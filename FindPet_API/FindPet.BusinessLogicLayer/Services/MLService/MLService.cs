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
        if (imageStream == null) throw new ArgumentNullException(nameof(imageStream), "Image stream cannot be null.");

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
        catch (ArgumentNullException ex)
        {
            logger.LogError($"Image stream cannot be null: {ex.Message}");
            return "Unknown";
        }
        catch (IOException ex)
        {
            logger.LogError($"IO error during prediction: {ex.Message}");
            return "Unknown";
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError($"Invalid operation during prediction: {ex.Message}");
            return "Unknown";
        }
        catch (FormatException ex)
        {
            logger.LogError($"Format error during prediction: {ex.Message}");
            return "Unknown";
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to predict pet type");
            return "Unknown";
        }
    }
}