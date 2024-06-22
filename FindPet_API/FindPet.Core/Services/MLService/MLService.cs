using FindPet.Infrastructure.Interfaces.IMLService;
using FindPet_API;


namespace FindPet.Core.Services.MLService;

public class MLService : IMLService
{

    public MLService()
    {
    }

    public async Task<string> PredictAsync(string filePath)
    {
        var sampleData = new PetMLModel.ModelInput()
        {
            ImageSource = await File.ReadAllBytesAsync(filePath),
        };

        var output = PetMLModel.Predict(sampleData);

        return output.PredictedLabel;

    }
}
