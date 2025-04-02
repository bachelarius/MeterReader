using Microsoft.AspNetCore.Http.HttpResults;

namespace MeterReaderAPI.MeterReadings;

public static class MeterReadingUploadsHandler {
    public static async Task<Results<Ok<object>, BadRequest<string>, StatusCodeHttpResult>> Handle(
        IFormFile file,
        IMeterReadingExtractorService meterReadingExtractor,
        IMeterReadingValidator meterReadingValidator) {
        if (file == null || file.Length == 0) {
            return TypedResults.BadRequest("No file uploaded");
        }

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)) {
            return TypedResults.BadRequest("File must be a CSV");
        }

        try {
            using var reader = new StreamReader(file.OpenReadStream());
            var csvContent = await reader.ReadToEndAsync();
            var meterReadingDtos = meterReadingExtractor.ExtractMeterReadings(csvContent).ToList();
            
            var validReadings = new List<MeterReading>();
            var invalidReadings = new List<MeterReadingCsvDTO>();

            foreach (var dto in meterReadingDtos) {
                var validatedReadings = meterReadingValidator.ExtractMeterReadings(dto).ToList();
                if (validatedReadings.Count != 0) {
                    validReadings.AddRange(validatedReadings);
                } else {
                    invalidReadings.Add(dto);
                }
            }

            return TypedResults.Ok<object>(new {
                TotalReadings = meterReadingDtos.Count,
                SuccessfulReadings = validReadings.Count,
                FailedReadings = invalidReadings.Count,
                Readings = validReadings,
                FailedReadingDetails = invalidReadings
            });
        } catch (Exception) {
            return TypedResults.StatusCode(500);
        }
    }
}



