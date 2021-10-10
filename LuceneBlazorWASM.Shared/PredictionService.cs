using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;
using Microsoft.ML;

namespace LuceneBlazorWASM.Shared
{
    public sealed class PredictionService
    {
        private static readonly PredictionService instance = new PredictionService();
        private static MLContext mlContext = new MLContext();
        private static Dictionary<string, PredictionEngine<MLBBaseballBatter, MLBHOFPrediction>> _predictionEngine = new Dictionary<string, PredictionEngine<MLBBaseballBatter, MLBHOFPrediction>>();

        private PredictionService()
        {
        }

        static PredictionService()
        {
        }


    }
}
