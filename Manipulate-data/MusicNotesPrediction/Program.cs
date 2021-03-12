using Microsoft.ML;
using Microsoft.ML.Runtime;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms;
using System;


namespace Microsoft.ML.MusicNotesPrediction
{
    class Program
    {
        const string dataPath = @"chorales-modified.csv";
        const string modelPath = @"MusicModel.zip";

        static void Main(string[] args)
        {
            Train();
        }

        public static void Train()
        {
            var pipeline = new LearningPipeline();

            //Upload Data
            pipeline.Add(new TextLoader<MusicNotes>(dataPath, useHeader: true, separator: ","));

            pipeline.Add(new Dictionarizer(("Note", "Label")));
            pipeline.Add(new ColumnConcatenator(outputColumn: "Features", 
                                                                        "Chorale", 
                                                                        "Key", 
                                                                        "N_60", 
                                                                        "N_61", 
                                                                        "N_62", 
                                                                        "N_63", 
                                                                        "N_64", 
                                                                        "N_65", 
                                                                        "N_66", 
                                                                        "N_67", 
                                                                        "N_68", 
                                                                        "N_69", 
                                                                        "N_70", 
                                                                        "N_71", 
                                                                        "N_72", 
                                                                        "N_73", 
                                                                        "N_74", 
                                                                        "N_75", 
                                                                        "N_76", 
                                                                        "N_77", 
                                                                        "N_78", 
                                                                        "N_79"));

            pipeline.Add(new StochasticDualCoordinateAscentClassifier());
            pipeline.Add(new PredictedLabelColumnOriginalValueConverter() { PredictedLabelColumn = "PredictedLabel" });

            PredictionModel<MusicNotes, MusicNotesPrediction> model = pipeline.Train<MusicNotes, MusicNotesPrediction>();

            model.WriteAsync(modelPath);

            MusicNotes note = new MusicNotes
            {
                Chorale = 1,
                Key=1,
                N_60=0,
                N_61 = 0,
                N_62 = 0,
                N_63 = 0,
                N_64 = 0,
                N_65 = 0,
                N_66 = 0,
                N_67 = 0,
                N_68 = 1,
                N_69 = 0,
                N_70 = 0,
                N_71 = 1,
                N_72 = 0,
                N_73 = 0,
                N_74 = 0,
                N_75 = 0,
                N_76 = 0,
                N_77 = 0,
                N_78 = 0,
                N_79 = 0
            };

            MusicNotesPrediction prediction = model.Predict(note);

            Console.WriteLine("Note is " + prediction.Note);
            Console.ReadLine();
        }
    }
}
