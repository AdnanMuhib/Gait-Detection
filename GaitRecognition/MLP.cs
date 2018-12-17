﻿using Emgu.CV;
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
        Matrix<float> testData;
        Matrix<float> testLabels;
        int InputLayers;
        int OutputLayers;

        float accuracy;
        float precision;
        float recall;
        float epochs;

        public MLP() {
            
        }
        // create the architecture of the Network
        public void CreateNetwork(int[] layers_with_perceptrons) {

        }
        // Load Complete Dataset
        public void LoadCSVData(String csvFilePath) {
            var data = File.ReadLines(csvFilePath).Select(x => x.Split(',')).ToArray();

            int rowcount = data.Length; 
            int columnCount = data[0].Length;

            // create matrix for Train Data
            trainData = new Matrix<float>(rowcount - 1, columnCount - 1);

            // create matrix for train classes 
            trainClasses = new Matrix<float>(rowcount - 1, 1);

            try
            {
                for (int i = 1; i < rowcount - 1; i++)
                {
                    for (int j = 0; j < columnCount; j++)
                    {
                        // last column as class label and store in training classes
                        if (j != columnCount - 1) {
                            trainData[i, j] = float.Parse(data[i][j]);
                        }
                        else 
                        {
                            trainClasses[i, 0] = float.Parse(data[i][j]);
                        }
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine("Error: " + e.Message);
            }
        }

        // Load only Training Dataset
        public void LoadCSVTrainData(String csvFilePath) {
            var data = File.ReadLines(csvFilePath).Select(x => x.Split(',')).ToArray();
        }

        // Load only Testing Data
        public void LoadCSVTestData(String csvFilePath)
        {
            var data = File.ReadLines(csvFilePath).Select(x => x.Split(',')).ToArray();
        }
        
        // Begin the training of ANN_MLP
        public void Train() {
            int trainSampleCount = 100;

            #region Generate the traning data and classes
            Matrix<float> trainData = new Matrix<float>(trainSampleCount, 2);
            Matrix<float> trainClasses = new Matrix<float>(trainSampleCount, 1);

            Image<Bgr, Byte> img = new Image<Bgr, byte>(500, 500);

            Matrix<float> sample = new Matrix<float>(1, 2);
            Matrix<float> prediction = new Matrix<float>(1, 1);

            Matrix<float> trainData1 = trainData.GetRows(0, trainSampleCount >> 1, 1);
            trainData1.SetRandNormal(new MCvScalar(200), new MCvScalar(50));
            Matrix<float> trainData2 = trainData.GetRows(trainSampleCount >> 1, trainSampleCount, 1);
            trainData2.SetRandNormal(new MCvScalar(300), new MCvScalar(50));

            Matrix<float> trainClasses1 = trainClasses.GetRows(0, trainSampleCount >> 1, 1);
            trainClasses1.SetValue(1);
            Matrix<float> trainClasses2 = trainClasses.GetRows(trainSampleCount >> 1, trainSampleCount, 1);
            trainClasses2.SetValue(2);
            #endregion

            using (Matrix<int> layerSize = new Matrix<int>(new int[] { 2, 5, 1 }))
            using (Mat layerSizeMat = layerSize.Mat)

            using (TrainData td = new TrainData(trainData, Emgu.CV.ML.MlEnum.DataLayoutType.RowSample, trainClasses))
            using (ANN_MLP network = new ANN_MLP())
            {
                network.SetLayerSizes(layerSizeMat);
                network.SetActivationFunction(ANN_MLP.AnnMlpActivationFunction.SigmoidSym, 0, 0);
                network.TermCriteria = new MCvTermCriteria(10, 1.0e-8);
                network.SetTrainMethod(ANN_MLP.AnnMlpTrainMethod.Backprop, 0.1, 0.1);
                network.Train(td, (int)Emgu.CV.ML.MlEnum.AnnMlpTrainingFlag.Default);

#if !NETFX_CORE
                String fileName = Path.Combine(Path.GetTempPath(), "ann_mlp_model.xml");
                network.Save(fileName);
                if (File.Exists(fileName))
                    File.Delete(fileName);
#endif

                for (int i = 0; i < img.Height; i++)
                {
                    for (int j = 0; j < img.Width; j++)
                    {
                        sample.Data[0, 0] = j;
                        sample.Data[0, 1] = i;
                        network.Predict(sample, prediction);

                        // estimates the response and get the neighbors' labels
                        float response = prediction.Data[0, 0];

                        // highlight the pixel depending on the accuracy (or confidence)
                        img[i, j] = response < 1.5 ? new Bgr(90, 0, 0) : new Bgr(0, 90, 0);
                    }
                }
            }

            // display the original training samples
            for (int i = 0; i < (trainSampleCount >> 1); i++)
            {
                PointF p1 = new PointF(trainData1[i, 0], trainData1[i, 1]);
                img.Draw(new CircleF(p1, 2), new Bgr(255, 100, 100), -1);
                PointF p2 = new PointF((int)trainData2[i, 0], (int)trainData2[i, 1]);
                img.Draw(new CircleF(p2, 2), new Bgr(100, 255, 100), -1);
            }

            Emgu.CV.UI.ImageViewer.Show(img);
        }

        // Predict on Given Dataset
        public void predict(String filePath) {

        }

        // Calculate and show the confusion matrix
        public void ConfusionMatrix() {

        }

        // Evaluation of Given Test and Train set to compute the accuracy
        public void Evaluate() {

        }

    }

}