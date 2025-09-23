using FindPet.BusinessLogicLayer.Interfaces.IMLService;

namespace FindPet.BusinessLogicLayer.Services.MLService;

public class MLService : IMLService
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
}