using FindPet.BusinessLogicLayer.Interfaces.IMLService;
using FindPet.Domain.Interfaces.ILoggerService;

namespace FindPet.BusinessLogicLayer.Services.MLService;

public class MLService(ILoggerManager logger) : IMLService
{
    public async Task<string> PredictAsync(string filePath)
    {
        var sampleData = new PetMLModel.ModelInput
        {
            ImageSource = await File.ReadAllBytesAsync(filePath)
        };

        var output = PetMLModel.Predict(sampleData);

        return output.PredictedLabel;
    }

    public async Task<string> PredictAsync(Stream imageStream)
    {
        try
        {
            using var memoryStream = new MemoryStream();
            await imageStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var sampleData = new PetMLModel.ModelInput
            {
                ImageSource = memoryStream.ToArray()
            };

            var output = await Task.Run(() => PetMLModel.Predict(sampleData));

            return output.PredictedLabel;
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed to predict from stream: {ex.Message}");
            return "Unknown";
        }
    }
}