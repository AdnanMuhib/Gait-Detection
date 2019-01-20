using Emgu.CV;
using Emgu.CV.ML;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaitRecognition
{
    class MLP
    {
        Matrix<float> trainData;
        Matrix<float> trainClasses;
        int TrainSamples;
        int TestSamples;
        private ANN_MLP nnet;
        Matrix<float> testData;
        Matrix<float> testClasses;
        Matrix<float> predictedClasses;
        int InputLayers;

        float accuracy;
        float precision;
        float recall;
        float epochs;

        public MLP() {
            nnet = new ANN_MLP();
        }
        // create the architecture of the Network
        public void CreateNetwork(int[] layers_with_perceptrons) {

        }

        // Load Complete Dataset
        public void LoadTrainData(String csvFilePath) {
            var data = File.ReadLines(csvFilePath).Select(x => x.Split(',')).ToArray();

            int rowcount = TrainSamples = data.Length; 
            int columnCount = data[0].Length;
            InputLayers = columnCount - 1;
            // create matrix for Train Data
            trainData = new Matrix<float>(rowcount - 1, columnCount - 1);

            // create matrix for train classes 
            trainClasses = new Matrix<float>(rowcount - 1, 1);

            try
            {
                for (int i = 0; i < rowcount - 1; i++)
                {
                    for (int j = 0; j < columnCount; j++)
                    {
                        // last column as class label and store in training classes
                        if (j != columnCount - 1) {
                            trainData[i, j] = float.Parse(data[i+1][j]);
                        }
                        else 
                        {
                            trainClasses[i, 0] = float.Parse(data[i+1][j]);
                        }
                    }
                }
                Console.WriteLine("Train Data Loaded Successfully....");
            }
            catch (Exception e) {
                Console.WriteLine("Error: " + e.Message);
            }
        }

        // Load only Testing Data
        public void LoadTestData(String csvFilePath)
        {
            var data = File.ReadLines(csvFilePath).Select(x => x.Split(',')).ToArray();

            int rowcount = data.Length;
            TestSamples = rowcount - 1;
            int columnCount = data[0].Length;
            InputLayers = columnCount - 1;
            // create matrix for Test Data
            testData = new Matrix<float>(rowcount - 1, columnCount - 1);

            // create matrix for Test classes 
            testClasses = new Matrix<float>(rowcount - 1, 1);
            predictedClasses = new Matrix<float>(rowcount - 1, 1);
            try
            {
                for (int i = 0; i < rowcount - 1; i++)
                {
                    for (int j = 0; j < columnCount; j++)
                    {
                        // last column as class label and store in Test classes
                        if (j != columnCount - 1)
                        {
                            testData[i, j] = float.Parse(data[i + 1][j]);
                        }
                        else
                        {
                            testClasses[i, 0] = float.Parse(data[i + 1][j]);
                        }
                    }
                }
                Console.WriteLine("Test Data Loaded Successfully....");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }
        
        // Begin the training of ANN_MLP
        public void Train() {
            int trainSampleCount = TrainSamples-1;

            using (Matrix<int> layerSize = new Matrix<int>(new int[] { InputLayers, 100, 1 }))
            using (Mat layerSizeMat = layerSize.Mat)

            using (TrainData td = new TrainData(trainData, Emgu.CV.ML.MlEnum.DataLayoutType.RowSample, trainClasses))
            {
                nnet.SetLayerSizes(layerSizeMat);
                nnet.SetActivationFunction(ANN_MLP.AnnMlpActivationFunction.SigmoidSym, 0.6, 1);
                nnet.TermCriteria = new MCvTermCriteria(430, 1.0e-8);
                nnet.SetTrainMethod(ANN_MLP.AnnMlpTrainMethod.Backprop, 0.1, 0);
                try
                {
                    nnet.Train(td, (int)Emgu.CV.ML.MlEnum.AnnMlpTrainingFlag.Default);
                    Console.WriteLine("Training Completed Successfully....");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Training Error:" + e.Message);
                }

#if !NETFX_CORE
                String fileName = "ann_mlp_model.xml";
                if (File.Exists(fileName)) // if model already exists delete model
                    File.Delete(fileName);
                nnet.Save(fileName); // save the model

                // Loading the Trained Model from File
                /*using (ANN_MLP mlp = new ANN_MLP())
                {
                    mlp.Load("ann_mlp_model.xml");
                    //mlp.Predict();
                }*/
#endif
            }
        }

        // Predict on loaded Dataset
        public void Predict() {

            Matrix<float> prediction = new Matrix<float>(1, 1);
            Matrix<float> sample = new Matrix<float>(1, InputLayers);
            int tp = 0;
            try
            {
                Console.WriteLine("______________ Testing Begins Here_____________");
                for (int i = 0; i < testData.Rows; i++)
                {
                    for (int j = 0; j < testData.Cols; j++)
                    {
                        sample[0, j] = testData[i, j];
                    }
                    nnet.Predict(sample, prediction);
                    predictedClasses[i, 0] = GetCloseValue(prediction.Data[0, 0]);

                    if (predictedClasses[i, 0] == testClasses[i, 0])
                        tp = tp + 1;
                    Console.WriteLine("Actual : " + testClasses[i, 0] + " Predicted: " + predictedClasses[i, 0]);
                }

                accuracy = (float)tp / (float)testClasses.Rows;
                Console.WriteLine("Accuracy: " + accuracy);
            }
            catch (Exception ex) {
                Console.WriteLine("Error occured Predicting..." + ex.Message);
            }
        }

        // Inference for Single Instance
        public int Inference(Matrix<float> sample) {
            Matrix<float> prediction = new Matrix<float>(1, 1);
            sample = new Matrix<float>(1, 36);
            try
            {
                nnet.Predict(sample, prediction);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("Prediction: " + prediction.Data[0, 0]);
            return GetCloseValue(prediction.Data[0, 0]);
        }
        // PostProcess the prediction
        int GetCloseValue(double n)
        {
            n = n + 0.5;
            n = Math.Floor(n);
            if (n == 2) {
                n = 3;
            }
            if (n > 6) {
                n = 6;
            }
            return (int)n;
            /*int u = 0;
            int a = (int)n;
            if ((n - a) < 0.5)
            {
                u = a;
            }
            else {
                u = a + 1;
            }
            if (u > 9) {
                u = 10;
            }
            return u;*/
        }
        // Calculate and show the confusion matrix
        public void ConfusionMatrix() {

        }

        // Evaluation of Given Test and Train set to compute the accuracy
        public void Evaluate() {

        }
        
        // Saving the Trained Model
        public void SaveModel(string ModelFileName) {
            #if !NETFX_CORE
                if (File.Exists(ModelFileName)) // if model already exists delete model
                    File.Delete(ModelFileName);
                nnet.Save(ModelFileName); // save the model
            #endif
        }

        // Loading the Trained Model
        public void LoadTrainedModel(String ModelFileName) {
            #if !NETFX_CORE
                // Loading the Trained Model from File
                nnet.Load(ModelFileName);
            #endif
        }
    }
}
