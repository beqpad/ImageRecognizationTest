using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCvSharp;
using OpenCvSharp.CPlusPlus;

namespace ImageRecognizationTest
{
    static class WashTagRecognize
    {
        private static WashTagDictionary washTagDictionary = new WashTagDictionary();
        private static WashTagGroupDictionary washTagGroupDictionary = new WashTagGroupDictionary();

        private class SURFResult
        {
            public List<CvSURFPoint> findPointList;
            public List<CvSURFPoint> basePointList;
            public CvSize templateSize;
            public CvPoint[] dstCorners;

            public SURFResult()
            {
                this.findPointList = new List<CvSURFPoint>();
                this.basePointList = new List<CvSURFPoint>();
            }
        }


        /// <summary>
        /// 認識処理を行う
        /// </summary>
        /// <param name="imagePath">認識対象の画像パス</param>
        /// <param name="isDebug">デバッグモード</param>
        /// <returns></returns>
        public static String Recognize(String imagePath, bool isDebug = false)
        {

            List<String> results = new List<string>();

            // 検出対象の画像を読み込み
            IplImage src = new IplImage(imagePath, LoadMode.GrayScale);

            using (IplImage tmpImage = new IplImage(src.Size, BitDepth.U8, 1))
            {
                // 1)検出前処理
                
                // ノイズ除去
                src.Smooth(src, SmoothType.Median);
                
                // エッジ強調
                //src.UnsharpMasking(src, 3);


                // 大津の手法による二値化処理
                // 大津, "判別および最小２乗基準に基づく自動しきい値選定法", 電子通信学会論文誌, Vol.J63-D, No.4, pp.349-356, 1980. 
                src.Threshold(tmpImage, 200, 250, ThresholdType.Otsu);

                src.Dispose();

                Dictionary<int, Dictionary<int, double>> shapeMatchResults = new Dictionary<int, Dictionary<int, double>>();

                List<int> answerFileNames = washTagGroupDictionary.Keys.ToList();
                foreach (var answerFileName in answerFileNames)
                {
                    //var washTagInfo = washTagDictionary[answerFileName];
                    var answerImagePath = String.Format(@"answer\group\{0}.png", answerFileName);

                    // 2) 検出処理
                    var resultSURF = SURF(tmpImage, answerImagePath, isDebug);


                    // 3) 検出候補の評価
                    string result = null;
                    

                    // その１：頂点がある場合
                    if (resultSURF.dstCorners != null)
                    {
                        // TODO:平面評価
                        //result = fileBaseName + " : " + washTagDictionary[fileBaseName];
                    }

                    // その２：形状マッチング
                    if (result == null && resultSURF.findPointList.Count > 0)
                    {
                        // ROIの1辺は、横に4つ位入る大きさで（何となくｗ）
                        CvSize roiSize = new CvSize(tmpImage.Width / 4, tmpImage.Width / 4);

                        List<double> matchResults = new List<double>();
                        for (int idx = 0; idx < resultSURF.findPointList.Count; idx++)
	                    {
                            var findPoint = resultSURF.findPointList[idx];
                            var basePoint = resultSURF.basePointList[idx];
                            //var offsetRetioX = basePoint.Pt.X / tmpImage.Width;
                            //var offsetRatioY = basePoint.Pt.Y / tmpImage.Height;

                            // ROIを設定
                            tmpImage.SetROI(
                                (int)(findPoint.Pt.X - roiSize.Width / 2), //- roiSize.Width * offsetRetioX),
                                (int)(findPoint.Pt.Y - roiSize.Height / 2), //- roiSize.Height * offsetRatioY),
                                roiSize.Width, roiSize.Height
                            );
                		    // Huモーメントによる形状マッチング [回転・スケーリング・反転に強い]
                            matchResults.Add(
                                CompareShapeMoment(tmpImage, answerImagePath)
                            );



                            //using (CvWindow win = new CvWindow("test", tmpImage))
                            //{
                            //    CvWindow.WaitKey();
                            //}

                            // ROIをリセット
                            tmpImage.ResetROI();
	                    }

                        
                        // 閾値以下だった場合に検出と見なす
                        var resultMin = matchResults.Min();
                        if (resultMin < 0.02)
                        {
                            var catNo = (int)(answerFileName / 100);

                            // カテゴリに値が無ければ確保
                            if (shapeMatchResults.ContainsKey(catNo) == false)
                            {
                                shapeMatchResults.Add(catNo, new Dictionary<int, double>());
                            }

                            shapeMatchResults[catNo].Add(answerFileName, resultMin);
                        }

                        Console.WriteLine("{0} : {1}", answerFileName, resultMin);

                    }
                    else
                    {
                        Console.WriteLine("{0} : {1}", answerFileName, -1);
                    }
                }


                // 4)検出結果の整理
                foreach (var categoryNo in shapeMatchResults.Keys)
                {
                    var matchResult = shapeMatchResults[categoryNo];

                    var min = matchResult.First<KeyValuePair<int, double>>((result) => result.Value == matchResult.Values.Min());
                    
                    var group = washTagGroupDictionary[min.Key];

                    if (group.Length > 0)
                    {
                        String id = null;
                        String description = null;
                        if (group.Length == 1)
                        {
                            // 1件の場合はそのままidとして用いる
                            id = String.Format("{0}", group[0]);
                            description = washTagDictionary[id].Description;
                        }
                        else
                        {
                            // 2件以上の場合は処理を切り替える
                            switch (min.Key)
                            {
                                case 100: // 洗濯機による洗濯の場合
                                    id = "101-105";
                                    description = "表示された液温を上限に洗濯機による洗濯が出来る。";
                                    break;

                                case 300: // アイロン掛けの場合
                                    id = "301-303";
                                    description = "表示された温度でアイロン掛けが出来る。";
                                    break;

                                case 401: // ドライ洗濯の場合
                                    id = "401-402";
                                    description = "ドライ洗濯が出来る。";
                                    break;

                                case 601: // 通常干し
                                    id = "601, 603";
                                    description = "つり干し、もしくは平干し。";
                                    break;

                                case 602: // 日陰干し
                                    id = "602, 604";
                                    description = "日陰のつり干し、もしくは日陰の平干し。";
                                    break;

                            }
                        }

                        if (id != null && description != null)
                        {
                            // 結果を格納
                            results.Add(
                                String.Format(isDebug ? "{0} : {1} ({2})" : "{0} : {1}", id, description, min.Value)
                            );
                        }
                    }

                    //var index = matchResult.FindIndex((x) =>
                    //{
                    //    return x == min;
                    //});

                    //var id = String.Format("{0:0}{1:00}", categoryNo, index + 1);
                    //var recognitionWashTag = washTagDictionary[id];

                    //// 結果を格納
                    //results.Add(
                    //    String.Format(isDebug ? "{0} : {1} ({2})" : "{0} : {1}", id, recognitionWashTag.Description, min)
                    //);
                    
                }

                

                // デバッグ表示
                if (isDebug)
                {
                    using (CvWindow win = new CvWindow("image", tmpImage))
                    {
                        CvWindow.WaitKey();
                    }
                }
            }


            return results.Count > 0 
                ? String.Join("\n", results.ToArray()) 
                : "検出する事が出来ませんでした。";
        }



        /// <summary>
        /// アンシャープマスク（エッジ強調）を行う
        /// </summary>
        /// <param name="src">元画像</param>
        /// <param name="dst">結果画像</param>
        /// <param name="k">強調</param>
        private static void UnsharpMasking(this CvArr src, CvArr dst, int k = 1)
        {
            float[] kernelElement = {
                -k/9.0f, -k/9.0f, -k/9.0f,
                -k/9.0f, 1+8*k/9.0f, -k/9.0f,
                -k/9.0f, -k/9.0f, -k/9.0f,                             
            };

            using (CvMat kernel = new CvMat(3, 3, MatrixType.F32C1, kernelElement))
            {
                src.Filter2D(dst, kernel);
            }

            return;
        }



        /// <summary>
        /// SURFによる検出処理を行う
        /// </summary>
        /// <param name="dstPath"></param>
        /// <param name="srcPath"></param>
        private static SURFResult SURF(IplImage image, String srcPath, bool isDebug = false)
        {
            SURFResult result = new SURFResult();

            // cvExtractSURF
            // SURFで対応点検出

            // call cv::initModule_nonfree() before using SURF/SIFT.
            CvCpp.InitModule_NonFree();
            
            using (IplImage obj = Cv.LoadImage(srcPath, LoadMode.GrayScale))
            //using (IplImage image = Cv.LoadImage(dstPath, LoadMode.GrayScale))
            using (IplImage objColor = Cv.CreateImage(obj.Size, BitDepth.U8, 3))
            using (IplImage correspond = Cv.CreateImage(new CvSize(image.Width, obj.Height + image.Height), BitDepth.U8, 1))
            {
                if (isDebug)
                {
                    Cv.CvtColor(obj, objColor, ColorConversion.GrayToBgr);

                    Cv.SetImageROI(correspond, new CvRect(0, 0, obj.Width, obj.Height));
                    Cv.Copy(obj, correspond);
                    Cv.SetImageROI(correspond, new CvRect(0, obj.Height, correspond.Width, correspond.Height));
                    Cv.Copy(image, correspond);
                    Cv.ResetImageROI(correspond);
                }

                // テンプレート画像の記録
                result.templateSize = obj.GetSize();


                // SURFの処理
                CvSeq<CvSURFPoint> objectKeypoints, imageKeypoints;
                CvSeq<IntPtr> objectDescriptors, imageDescriptors;
                System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
                {
                    using (CvMemStorage storage = Cv.CreateMemStorage(0))
                    {
                        //CvSURFParams param = new CvSURFParams(20, true);

                        Cv.ExtractSURF(obj, null, out objectKeypoints, out objectDescriptors, storage, new CvSURFParams(20, true));
                        //Console.WriteLine("Object Descriptors: {0}", objectDescriptors.Total);

                        Cv.ExtractSURF(image, null, out imageKeypoints, out imageDescriptors, storage, new CvSURFParams(20000, true));
                        //Console.WriteLine("Image Descriptors: {0}", imageDescriptors.Total);
                    }
                }
                watch.Stop();
                //Console.WriteLine("Extraction time = {0}ms", watch.ElapsedMilliseconds);
                watch.Reset();
                watch.Start();


                // シーン画像にある局所画像の領域を線で囲む
                //CvPoint[] srcCorners = new CvPoint[4]{
                //    new CvPoint(0,0), 
                //    new CvPoint(obj.Width,0), 
                //    new CvPoint(obj.Width, obj.Height), 
                //    new CvPoint(0, obj.Height)
                //};
                //CvPoint[] dstCorners = LocatePlanarObject(
                //    objectKeypoints, objectDescriptors, 
                //    imageKeypoints, imageDescriptors, 
                //    srcCorners
                //);
                //if (dstCorners != null)
                //{
                //    // 検出した領域の頂点
                //    result.dstCorners = dstCorners;

                //    for (int i = 0; i < 4; i++)
                //    {
                //        CvPoint r1 = dstCorners[i % 4];
                //        CvPoint r2 = dstCorners[(i + 1) % 4];
                //        if (isDebug)
                //        {
                //            Cv.Line(correspond, new CvPoint(r1.X, r1.Y + obj.Height), new CvPoint(r2.X, r2.Y + obj.Height), CvColor.Black);
                //        }
                //    }
                //}

                
                // 対応点同士を線で引く
                int[] ptPairs = FindPairs(objectKeypoints, objectDescriptors, imageKeypoints, imageDescriptors);
                for (int i = 0; i < ptPairs.Length; i += 2)
                {
                    CvSURFPoint r1 = Cv.GetSeqElem<CvSURFPoint>(objectKeypoints, ptPairs[i]).Value;
                    CvSURFPoint r2 = Cv.GetSeqElem<CvSURFPoint>(imageKeypoints, ptPairs[i + 1]).Value;

                    // 対応点を格納
                    result.basePointList.Add(r1);
                    result.findPointList.Add(r2);

                    if (isDebug)
                    {
                        Cv.Line(correspond, r1.Pt, new CvPoint(Cv.Round(r2.Pt.X), Cv.Round(r2.Pt.Y + obj.Height)), CvColor.Black);
                    }
                }                


                //// 特徴点の場所に円を描く
                //for (int i = 0; i < objectKeypoints.Total; i++)
                //{
                //    CvSURFPoint r = Cv.GetSeqElem<CvSURFPoint>(objectKeypoints, i).Value;
                //    CvPoint center = new CvPoint(Cv.Round(r.Pt.X), Cv.Round(r.Pt.Y));
                //    int radius = Cv.Round(r.Size * (1.2 / 9.0) * 2);
                //    Cv.Circle(objColor, center, radius, CvColor.Red, 1, LineType.AntiAlias, 0);
                //}

                watch.Stop();
                //Console.WriteLine("Drawing time = {0}ms", watch.ElapsedMilliseconds);


                // ウィンドウに表示
                if (isDebug)
                {
                    //using (CvWindow winObject = new CvWindow("Object", WindowMode.AutoSize, objColor))
                    using (CvWindow winCorrespond = new CvWindow("Object Correspond", WindowMode.AutoSize, correspond))
                    {
                        CvWindow.WaitKey(0);
                    }
                }
            }

            return result;
        }



        /// <summary>
        /// Huモーメントによる形状比較を行う
        /// </summary>
        /// <param name="sourceImage"></param>
        /// <param name="templateImagePath"></param>
        /// <returns>比較結果（0.0で完全一致）</returns>
        private static double CompareShapeMoment(IplImage sourceImage, String templateImagePath, MatchShapesMethod matchShapesMethod = MatchShapesMethod.I1)
        {
            double result = 1;

            using (IplImage template = Cv.LoadImage(templateImagePath, LoadMode.GrayScale))
            {
                //result = Cv.MatchShapes(sourceImage, template, matchShapesMethod);

                result = Cv.MatchShapes(sourceImage, template, MatchShapesMethod.I1)
                    + Cv.MatchShapes(sourceImage, template, MatchShapesMethod.I2)
                    + Cv.MatchShapes(sourceImage, template, MatchShapesMethod.I3);
            }

            return result;
        }



        /// <summary>
        /// SURFで検出した特徴記述の比較を行う
        /// </summary>
        /// <param name="d1Ptr">Cではconst float*</param>
        /// <param name="d2Ptr">Cではconst float*</param>
        /// <param name="best"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private static unsafe double CompareSURFDescriptors(IntPtr d1Ptr, IntPtr d2Ptr, double best, int length)
        {
            //Debug.Assert(length % 4 == 0);

            double totalCost = 0;

            // ポインタでのアクセスの代わりに配列にコピーしてからやる。            
            /*float[] d1 = new float[length];
            float[] d2 = new float[length];
            Marshal.Copy(d1Ptr, d1, 0, length);
            Marshal.Copy(d2Ptr, d2, 0, length);*/

            // 遅くて問題ならunsafeとか
            float* d1 = (float*)d1Ptr;
            float* d2 = (float*)d2Ptr;

            double t0, t1, t2, t3;
            for (int i = 0; i < length; i += 4)
            {
                t0 = d1[i] - d2[i];
                t1 = d1[i + 1] - d2[i + 1];
                t2 = d1[i + 2] - d2[i + 2];
                t3 = d1[i + 3] - d2[i + 3];
                totalCost += t0 * t0 + t1 * t1 + t2 * t2 + t3 * t3;
                if (totalCost > best) break;
            }

            return totalCost;
        }



        /// <summary>
        /// 単純な最近傍
        /// </summary>
        /// <param name="vec">Cではconst float*</param>
        /// <param name="laplacian"></param>
        /// <param name="model_keypoints"></param>
        /// <param name="model_descriptors"></param>
        /// <returns></returns>
        private static int NaiveNearestNeighbor(
            IntPtr vec, int laplacian, 
            CvSeq<CvSURFPoint> model_keypoints, CvSeq<IntPtr> model_descriptors)
        {
            
            int length = (int)(model_descriptors.ElemSize / sizeof(float));
            int neighbor = -1;
            
            double dist1 = 1e6;
            double dist2 = 1e6;

            using(CvSeqReader<float> reader = new CvSeqReader<float>())
            using (CvSeqReader<CvSURFPoint> kreader = new CvSeqReader<CvSURFPoint>())
            {
                Cv.StartReadSeq(model_keypoints, kreader, false);
                Cv.StartReadSeq(model_descriptors, reader, false);

                IntPtr mvec;
                CvSURFPoint kp;
                double d;


                for (int i = 0; i < model_descriptors.Total; i++)
                {
                    // const CvSURFPoint* kp = (const CvSURFPoint*)kreader.ptr; が結構曲者。
                    // OpenCvSharpの構造体はFromPtrでポインタからインスタンス生成できるようにしてるので、こう書ける。
                    kp = CvSURFPoint.FromPtr(kreader.Ptr);

                    mvec = reader.Ptr;
                    Cv.NEXT_SEQ_ELEM(kreader.Seq.ElemSize, kreader);
                    Cv.NEXT_SEQ_ELEM(reader.Seq.ElemSize, reader);
                    if (laplacian != kp.Laplacian)
                    {
                        continue;
                    }

                    // SURF特徴点の比較を行う
                    d = CompareSURFDescriptors(vec, mvec, dist2, length);
                    if (d < dist1)
                    {
                        dist2 = dist1;
                        dist1 = d;
                        neighbor = i;
                    }
                    else if (d < dist2)
                    {
                        dist2 = d;
                    }
                }
            }

            return (dist1 < dist2 * 0.6) ? neighbor : -1;
        }



        /// <summary>
        /// 特徴点のペアを見つけて配列として返す
        /// </summary>
        /// <param name="objectKeypoints"></param>
        /// <param name="objectDescriptors"></param>
        /// <param name="imageKeypoints"></param>
        /// <param name="imageDescriptors"></param>
        /// <returns></returns>
        private static int[] FindPairs(
            CvSeq<CvSURFPoint> objectKeypoints, CvSeq<IntPtr> objectDescriptors, 
            CvSeq<CvSURFPoint> imageKeypoints, CvSeq<IntPtr> imageDescriptors)
        {
            List<int> ptPairs = new List<int>();

            using(CvSeqReader<float> descReader = new CvSeqReader<float>())
            using (CvSeqReader<CvSURFPoint> keyReader = new CvSeqReader<CvSURFPoint>())
            {
                Cv.StartReadSeq(objectDescriptors, descReader);
                Cv.StartReadSeq(objectKeypoints, keyReader);

                for (int i = 0; i < objectDescriptors.Total; i++)
                {
                    CvSURFPoint keypoint = CvSURFPoint.FromPtr(keyReader.Ptr);
                    IntPtr descriptor = descReader.Ptr;

                    Cv.NEXT_SEQ_ELEM(keyReader.Seq.ElemSize, keyReader);
                    Cv.NEXT_SEQ_ELEM(descReader.Seq.ElemSize, descReader);

                    // 単純な最近傍によって類似度の高い要素を探す
                    int nearestNeighbor = NaiveNearestNeighbor(descriptor, keypoint.Laplacian, imageKeypoints, imageDescriptors);
                    if (nearestNeighbor >= 0)
                    {
                        ptPairs.Add(i);
                        ptPairs.Add(nearestNeighbor);
                    }
                }
            }

            return ptPairs.ToArray();
        }




        /// <summary>
        /// ホモグラフィ行列を求めて、位置と体勢を大まかに算出
        /// </summary>
        /// <param name="objectKeypoints"></param>
        /// <param name="objectDescriptors"></param>
        /// <param name="imageKeypoints"></param>
        /// <param name="imageDescriptors"></param>
        /// <param name="srcCorners"></param>
        /// <returns></returns>
        private static CvPoint[] LocatePlanarObject(
                CvSeq<CvSURFPoint> objectKeypoints, CvSeq<IntPtr> objectDescriptors,
                CvSeq<CvSURFPoint> imageKeypoints, CvSeq<IntPtr> imageDescriptors,
                CvPoint[] srcCorners)
        {
            CvMat h = new CvMat(3, 3, MatrixType.F64C1);            

            // ペアを検索
            int[] ptPairs = FindPairs(objectKeypoints, objectDescriptors, imageKeypoints, imageDescriptors);
            int n = ptPairs.Length / 2;

            // ペアが4つ以上見つからない場合は透視変換行列を算出出来ないため終了
            if (n < 4) return null;


            // 2枚の画像間の特徴点における透視変換（ホモグラフィ）行列をRANSACアルゴリズムを用いて探す
            CvPoint2D32f[] pt1 = new CvPoint2D32f[n];
            CvPoint2D32f[] pt2 = new CvPoint2D32f[n];
            for (int i = 0; i < n; i++)
            {
                pt1[i] = (Cv.GetSeqElem<CvSURFPoint>(objectKeypoints, ptPairs[i * 2])).Value.Pt;
                pt2[i] = (Cv.GetSeqElem<CvSURFPoint>(imageKeypoints, ptPairs[i * 2 + 1])).Value.Pt;
            }
            using (CvMat pt1Mat = new CvMat(1, n, MatrixType.F32C2, pt1))
            using (CvMat pt2Mat = new CvMat(1, n, MatrixType.F32C2, pt2))
            { 
                if (Cv.FindHomography(pt1Mat, pt2Mat, h, HomographyMethod.Ransac, 5) == 0) {
                    // 透視変換行列が得られない場合は終了
                    return null;
                }
            }

            // 算出された3x3のホモグラフィ行列から座標値を求める
            CvPoint[] dstCorners = new CvPoint[4];
            for (int i = 0; i < 4; i++)
            {
                // 元座標
                double x = srcCorners[i].X;
                double y = srcCorners[i].Y;
                // スケール
                double Z = 1.0 / (h[6] * x + h[7] * y + h[8]);
                // 変換後の座標
                double X = (h[0] * x + h[1] * y + h[2]) * Z;
                double Y = (h[3] * x + h[4] * y + h[5]) * Z;

                dstCorners[i] = new CvPoint(Cv.Round(X), Cv.Round(Y));
            }

            return dstCorners;
        }
    }

    
}
