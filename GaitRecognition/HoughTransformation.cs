using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaitRecognition
{
    /// <summary>
	/// The Class to Apply Hough Transformation
	///  to the Stick Image
	/// </summary>
	public class HoughTransformation
    {
        int backLineX1 = 0, backLineX2 = 0, backLineY1 = 0, backLineY2 = 0, midStick = 0;
        int winX1 = 0, winX2 = 0, winY1 = 0, winY2 = 0;
        int finalBackLineX1 = 0, finalBackLineY1 = 0;
        int finalBackLineX2 = 0, finalBackLineY2 = 0;
        int middleLineX = 0, middleLineY = 0;
        int secondLeg = 0;
        public int WidtH = 0;
        public int HeighT = 0;
        private Bitmap blackBmp;
        private BitmapData blackData;

        //int ArrayList

        public HoughTransformation()
        {
            // 
            // TODO: Add constructor logic here
            //
        }
        /// <summary>
        /// ///////////////
        /// ///////////////////

        public Bitmap Transformation(Bitmap b)

        {
            Bitmap bmp = (Bitmap)b.Clone();
            Bitmap blackBmp = (Bitmap)b.Clone();

            BitmapData blackData = blackBmp.LockBits(new Rectangle(0, 0, blackBmp.Width, blackBmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int stride = bmpData.Stride;
            int stride1 = blackData.Stride;

            int offset = stride - bmp.Width * 3;
            System.IntPtr scan0 = bmpData.Scan0;
            System.IntPtr blackScan0 = blackData.Scan0;

            WidtH = bmp.Width;
            HeighT = bmp.Height;
            int rows = bmp.Height;
            int cols = bmp.Width;
            int firstX = 0, firstY = 0, lastX = 0, lastY = 0;
            int p = 0;
            int winRows = 0, winCols = 0;
            int part = 0;
            ArrayList[] linesCoordinates = new ArrayList[5];
            ArrayList[] counter = new ArrayList[1000];
            int[][] lineEndsXY = new int[5][];
            int[] windowXY = new int[4];
            int countPix = 0;
            ////////////////////////////////////////

            ///////////////Blackening Of Blackbmp/////////////
            ///
            unsafe
            {
                byte* blackPtr = (byte*)(void*)blackScan0;
                for (int i = 0; i < bmp.Height; i++)
                {
                    for (int j = 0; j < bmp.Width; j++)
                    {
                        blackPtr = (byte*)(void*)blackScan0;
                        blackPtr[(i * stride) + j * 3] = 0;
                        blackPtr[(i * stride) + j * 3 + 1] = 0;
                        blackPtr[(i * stride) + j * 3 + 2] = 0;
                        //blackPtr[0]=blackPtr[1]=blackPtr[2] = 0;

                        blackPtr += 3;
                    }
                    blackPtr += offset;
                }
                ///
                ///////////////////////////////////////////////////
                ////////////////////////Transformation Starts ////////
                ///

                for (part = 1; part <= 5; part++)
                {
                    winX1 = winY1 = winX2 = winY2 = 0;
                    if (part == 1)
                    {
                        backBoneLine();
                    }
                    else if (part == 2)
                    {
                        rightHandLine();
                    }
                    else if (part == 3)
                    {
                        leftHandLine();
                    }
                    else if (part == 4)
                    {
                        rightLegLine();
                    }
                    /*else if(part == 5)
                    {	
                        secondRightLegLine();
                    }*/
                    else if (part == 5)
                    {
                        leftLegLine();
                    }
                    /*else if (part == 7)
					{
						secondLeftLegLine();
					}*/

                    /*listBox13.Items.Add (backLineX1);
					listBox13.Items.Add (backLineY1);
					listBox13.Items.Add (backLineX2);
					listBox13.Items.Add (backLineY2);*/

                    /*  listBox14.Items.Add (finalBackLineX1);
						listBox14.Items.Add (finalBackLineY1);
						listBox14.Items.Add (finalBackLineX2);
						listBox14.Items.Add (finalBackLineY2);
					*/

                    winRows = Math.Abs(winY1 - winY2);
                    winCols = Math.Abs(winX1 - winX2);
                    //int diagonal = (int) Math.Sqrt (Math.Pow(winRows,2)+Math.Pow(winCols,2));
                    int diagonal = (int)Math.Sqrt(Math.Pow(rows, 2) + Math.Pow(cols, 2));
                    /*listBox23.Items.Add (part);
					listBox23.Items.Add (diagonal);
					listBox23.Items.Add (" ");*/
                    int angle = 180;
                    countPix = 0;
                    ArrayList[,] accumulator = new ArrayList[diagonal, angle];
                    for (int d = 0; d < diagonal; d++)
                        for (int a = 0; a < angle; a++)
                        {
                            accumulator[d, a] = new ArrayList();
                        }
                    unsafe
                    {

                        byte* ptr = (byte*)(void*)scan0;
                        int firstPixel = 0;

                        ////////////////////////////////
                        /*	if (part == 4 || part ==6)
							{
								for(int y= winY1;y<=winY2;y++)
								{
									for(int x=winX1;x<=winX2;x++)
									{
							
										if (ptr[(y*stride)+x*3] == 255 ) //&&ptr[1] == 255&&ptr[2] == 255) //for white pixel
										{
											counter[c] = new ArrayList();
											int [] pixel = {x,y};
											counter[c].Add (pixel);
											listBox2.Items.Add (x);
											listBox2.Items.Add (y);
											c++;
										}
									}  
								}
								ArrayList a = counter[c/2];
								int [] pix = (int[]) a[0];
							
								//winX2 = pix[0];
								winY2 = pix[1];
								ArrayList b1 = counter[c/2];
								if (part==6)
									b1 = counter[c/2+1];
								pix = (int[]) b1[0];
								secondLeg = pix [1]; 

							}// End of if
						*/    /////////////////////////////////////


                        for (int y = winY1; y <= winY2; y++)
                        {
                            for (int x = winX1; x <= winX2; x++)
                            {
                                if (ptr[(y * stride) + x * 3] == 255) //&&ptr[1] == 255&&ptr[2] == 255) //for white pixel
                                //if (ptr[(y * stride) + x * 3] == 255 &&ptr[1] == 255&&ptr[2] == 255) //for white pixel
                                {
                                    if (firstPixel == 0)
                                    {
                                        firstX = x;
                                        firstY = y;
                                        firstPixel++;
                                    }
                                    lastX = x;
                                    lastY = y;
                                    int[] pixel = { x, y };
                                    //listBox1.Items.Add (pixel[0]);
                                    //listBox2.Items.Add (pixel[1]);
                                    countPix++;
                                    for (int m = 0; m < angle; m++)
                                    {
                                        p = 0;
                                        ///Polar Equation in terms of pixel coordinates.
                                        p = (int)((x * Math.Cos((Math.PI * m) / 180) + y * Math.Sin((Math.PI * m) / 180)));
                                        if (p > 0 && p < diagonal)
                                        {
                                            accumulator[p, m].Add(pixel);
                                            //int d = (int)Math.Sqrt(Math.Pow ((winX2-winX1),2)+Math.Pow  ((winY2-winY1),2));
                                            //blackPtr[((p*(winY2-winy1)/diagonal)*stride)+((m*(winX2-winX1)/360)*3)] +=5; 

                                            int d = (int)Math.Sqrt(Math.Pow(bmp.Width, 2) + Math.Pow(bmp.Height, 2));
                                            //int b1 = (int)(bmp.Width *(p/d));																
                                            //int a1 = (int) ((m*(bmp.Height ))/360);

                                            int a1 = (int)(p * (bmp.Height) / d);

                                            int b1 = (int)(m * bmp.Width / 360);

                                            //Hough TRansformation Graph Drawing
                                            /*blackPtr[(a1*stride1)+b1*3] += 5;
											blackPtr[(a1*stride1)+(b1*3)+1] += 5;
											blackPtr[(a1*stride1)+(b1*3)+2] += 5;*/


                                        }
                                    } // End of for of m 
                                }//End of IF

                            }//End of x loop

                        }//End of y loop
                         /////////End of calculations for (p,theata)
                         ///

                        /*listBox7.Items.Add (firstX);
						listBox7.Items.Add (firstY);
						listBox7.Items.Add (lastX);
						listBox7.Items.Add (lastY);*/

                        if (countPix > 10)
                        {
                            ///////////////////////////////////////
                            //To cut the foot length 
                            if (part == 4 || part == 5)
                            {
                                lastY = lastY - 5;
                            }
                            ////////////////////////////////////////



                            ////////Getting points on starting and ending scan line/////

                            ////////////////////////////////////////////
                            ///

                            int sum = 0;
                            ArrayList co = new ArrayList();
                            for (int d = 0; d < diagonal; d++)
                                for (int a = 0; a < angle; a++)
                                {
                                    co.Add(accumulator[d, a].Count);
                                    sum += accumulator[d, a].Count;
                                }
                            co.Sort();
                            //ArrayList linePixels=accumulator[0,0];
                            //textBox3.Text = co[co.Count -1].ToString ();
                            /*listBox23.Items.Add (co[co.Count -1]);
							listBox23.Items.Add (co[co.Count -2]);
							listBox23.Items.Add (co[co.Count -3]);
							listBox23.Items.Add (co[co.Count -4]);
							listBox23.Items.Add (co[co.Count -5]);
							listBox23.Items.Add (co[co.Count -6]);
							listBox23.Items.Add (co[co.Count -7]);*/

                            int polar = 0, theata = 0, count = 0;
                            for (int d = 0; d < diagonal; d++)
                                for (int a = 0; a < angle; a++)
                                {
                                    if (accumulator[d, a].Count > 0 && accumulator[d, a].Count == (int)co[co.Count - 1] && count == 0)
                                    {
                                        count++;
                                        polar = d;
                                        theata = a;
                                        /*listBox6.Items.Add (part);
										listBox6.Items.Add (polar);
										listBox6.Items.Add (theata);*/
                                        ArrayList linePixels = accumulator[d, a];
                                        //linePixels = accumulator[d,a];
                                        for (p = 0; p < linePixels.Count; p++)
                                        {

                                            int[] pix = (int[])linePixels[p];
                                            if (part == 1)
                                            {
                                                /*listBox21.Items.Add (pix[0]);
												listBox21.Items.Add (pix[1]);*/
                                            }
                                            /////////////////////
                                            ///
                                            /*	blackPtr = (byte*)(void*)blackScan0;
												blackPtr[(pix[1]*stride)+pix[0]*3] = 255;
												blackPtr[(pix[1]*stride)+(pix[0]*3)+1] = 255;
												blackPtr[(pix[1]*stride)+(pix[0]*3)+2] = 255;
										*/    /////////////////////								
                                        }
                                    }
                                }//End For Loop
                                 /////////////Extracting exact starting and ending points//////
                                 ///

                            if (part == 1)
                            {
                                int[] pix = calculateXYHorizontal(firstX, firstY, polar, theata);
                                firstX = pix[0];
                                firstY = pix[1];
                                pix = calculateXYHorizontal(lastX, lastY, polar, theata);
                                lastX = pix[0];
                                lastY = pix[1];

                                finalBackLineX1 = firstX;
                                finalBackLineY1 = firstY;
                                finalBackLineX2 = lastX;
                                finalBackLineY2 = lastY;


                            }// End Of First Part


                            if (part == 3)
                            {
                                ////////////
                                if (firstX < lastX)
                                {

                                    //For Open End
                                    int[] pix = calculateXYHorizontal(firstX, firstY, polar, theata);
                                    //	firstX = pix[0];
                                    //	firstY = pix[1];
                                    //For End with Back Bone
                                    pix = calculateXYHand(lastX, lastY, polar, theata);
                                    lastX = pix[0];
                                    lastY = pix[1];

                                }///// End of If
                                 //////////
                                 ///
                                else
                                {

                                    //For End with Back Bone
                                    int[] pix = calculateXYHand(firstX, firstY, polar, theata);
                                    firstX = pix[0];
                                    firstY = pix[1];
                                    //For Open End
                                    //	pix = calculateXYHorizontal(lastX,lastY,polar,theata);
                                    //	lastX = pix[0];
                                    //	lastY = pix[1];


                                }///// End of Else

                            }//End of Part 3 



                            ///////For Part 2/////////////
                            ///


                            if (part == 2)
                            {
                                ////////////
                                if (lastX < firstX)
                                {
                                    //For Open End
                                    int[] pix = calculateXYHorizontal(firstX, firstY, polar, theata);
                                    //	firstX = pix[0];
                                    //	firstY = pix[1];	
                                    //For  End with Back Bone
                                    pix = calculateXYHand(lastX, lastY, polar, theata);
                                    lastX = pix[0];
                                    lastY = pix[1];


                                }///// End of If
                                 //////////
                                 ///
                                else
                                {
                                    // For  Open End								
                                    int[] pix = calculateXYHorizontal(lastX, lastY, polar, theata);
                                    //	lastX = pix[0];
                                    //	lastY = pix[1];
                                    // For  End with Back Bone
                                    pix = calculateXYHand(firstX, firstY, polar, theata);
                                    firstX = pix[0];
                                    firstY = pix[1];

                                }///// End of Else

                            }//End Of Part 2


                            ////////////////////////////

                            if (part == 4 || part == 5)
                            {
                                int[] pix = calculateXYHorizontal(lastX, lastY, polar, theata);
                                lastX = pix[0];
                                lastY = pix[1];

                                firstX = finalBackLineX2;
                                firstY = finalBackLineY2;

                            }//End Of Part 4 & 5
                             //////////////////////////////////////////////////////////////
                            int inc = 0;
                            if (part == 1)
                            {
                                inc = finalBackLineY2;
                                int countPixel = 1;
                                ptr = (byte*)(void*)scan0;
                                while (countPixel <= 2)//(ptr[(inc*stride)+lastX*3]==255&&countPixel<=2)
                                {
                                    countPixel = 1;
                                    for (int i = ((lastX - 5) * 3); i <= ((lastX + 5) * 3); i += 3)
                                    {
                                        ptr = (byte*)(void*)scan0;
                                        /////////////////////////
                                        /*if((inc*stride)+i > bmp.Width*3+stride )
										{
											if ( bmp.Width*3==255)
											{
												//listBox4.Items.Add (i/3);
												countPixel++;
											}
										}*/
                                        ////////////////////////
                                        if (ptr[(inc * stride) + i] == 255)
                                        {
                                            //listBox4.Items.Add (i/3);
                                            countPixel++;
                                        }
                                    }
                                    //listBox4.Items.Add (countPixel);
                                    if (countPixel <= 3)
                                    {
                                        //blackPtr[(inc*stride)+lastX*3] = 255;
                                        //blackPtr[(inc*stride)+(lastX*3)+1] = 255;
                                        //blackPtr[(inc*stride)+(lastX*3)+2] = 255;
                                        finalBackLineY2 = inc;
                                    }
                                    inc++;

                                }//End of while
                                firstX = finalBackLineX1 = (finalBackLineX1 + finalBackLineX2) / 2;
                                lastX = finalBackLineX2 = (finalBackLineX1 + finalBackLineX2) / 2;
                                lastY = finalBackLineY2;
                                /*listBox11.Items.Add(finalBackLineX1);
								listBox11.Items.Add(finalBackLineY1);
								listBox11.Items.Add(finalBackLineX2);
								listBox11.Items.Add(finalBackLineY2);*/

                                //listBox8.Items.Add (c1);
                            }// End Of If part ==1 



                            //Drawing the line
                            float dx = Math.Abs(firstX - lastX);
                            float dy = Math.Abs(firstY - lastY);
                            float py = 2 * dy - dx;
                            float px = 2 * dx - dy;
                            float twoDy = 2 * dy;
                            float twoDx = 2 * dx;
                            float twoDyDx = 2 * (dy - dx);
                            float twoDxDy = 2 * (dx - dy);
                            int temporaryX, temporaryY, xEnd, yEnd;
                            float slope;

                            if (dx == 0)
                            {
                                temporaryX = lastX;
                                for (temporaryY = firstY; temporaryY <= lastY; temporaryY++)
                                {
                                    blackPtr = (byte*)(void*)blackScan0;
                                    blackPtr[(temporaryY * stride) + temporaryX * 3] = 255;
                                    blackPtr[(temporaryY * stride) + (temporaryX * 3) + 1] = 255;
                                    blackPtr[(temporaryY * stride) + (temporaryX * 3) + 2] = 255;
                                }
                            }
                            else
                            {
                                slope = (float)Math.Abs(dy / dx);
                                if (slope <= 1)
                                {
                                    if (firstX > lastX)
                                    {
                                        temporaryX = lastX;
                                        temporaryY = lastY;
                                        xEnd = firstX;
                                        while (temporaryX < xEnd)
                                        {
                                            temporaryX++;
                                            if (py < 0)
                                                py += twoDy;
                                            else
                                            {
                                                temporaryY--;
                                                py += twoDyDx;
                                            }

                                            //set Pixel
                                            blackPtr = (byte*)(void*)blackScan0;
                                            blackPtr[(temporaryY * stride) + temporaryX * 3] = 255;
                                            blackPtr[(temporaryY * stride) + (temporaryX * 3) + 1] = 255;
                                            blackPtr[(temporaryY * stride) + (temporaryX * 3) + 2] = 255;
                                        }
                                    }
                                    else
                                    {
                                        temporaryX = firstX;
                                        temporaryY = firstY;
                                        xEnd = lastX;
                                        while (temporaryX < xEnd)
                                        {
                                            temporaryX++;
                                            if (py < 0)
                                                py += twoDy;
                                            else
                                            {
                                                temporaryY++;
                                                py += twoDyDx;
                                            }

                                            //set Pixel
                                            blackPtr = (byte*)(void*)blackScan0;
                                            blackPtr[(temporaryY * stride) + temporaryX * 3] = 255;
                                            blackPtr[(temporaryY * stride) + (temporaryX * 3) + 1] = 255;
                                            blackPtr[(temporaryY * stride) + (temporaryX * 3) + 2] = 255;
                                        }
                                    }
                                }
                                else
                                {
                                    if (firstY < lastY)
                                    {
                                        if (firstX < lastX)
                                        {
                                            temporaryX = firstX;
                                            temporaryY = firstY;
                                            yEnd = lastY;

                                            while (temporaryY < yEnd)
                                            {
                                                temporaryY++;
                                                if (px < 0)
                                                    px += twoDx;
                                                else
                                                {
                                                    temporaryX++;
                                                    px += twoDxDy;
                                                }
                                                //set Pixel
                                                blackPtr = (byte*)(void*)blackScan0;
                                                blackPtr[(temporaryY * stride) + temporaryX * 3] = 255;
                                                blackPtr[(temporaryY * stride) + (temporaryX * 3) + 1] = 255;
                                                blackPtr[(temporaryY * stride) + (temporaryX * 3) + 2] = 255;
                                            }
                                        }
                                        else
                                        {
                                            temporaryX = firstX;
                                            temporaryY = firstY;
                                            yEnd = lastY;

                                            while (temporaryY < yEnd)
                                            {
                                                temporaryY++;
                                                if (px < 0)
                                                    px += twoDx;
                                                else
                                                {
                                                    temporaryX--;
                                                    px += twoDxDy;
                                                }
                                                //set Pixel
                                                blackPtr = (byte*)(void*)blackScan0;
                                                blackPtr[(temporaryY * stride) + temporaryX * 3] = 255;
                                                blackPtr[(temporaryY * stride) + (temporaryX * 3) + 1] = 255;
                                                blackPtr[(temporaryY * stride) + (temporaryX * 3) + 2] = 255;
                                            }
                                        }
                                    }
                                }
                            }// unsafe of bmp	
                        }//Doubt Ful Braces
                         /////////////////////End Of Drawing //////////////////////

                    }//End of if countPix>10
                    if (countPix < 10)
                    {
                        firstX = firstY = lastX = lastY = 0;
                    }

                    /*listBox5.Items.Add (firstX);
					listBox5.Items.Add (firstY);
					listBox5.Items.Add (lastX);
					listBox5.Items.Add (lastY);*/
                    lineEndsXY[part - 1] = new int[] { firstX, firstY, lastX, lastY };

                    /*for(int p1 = 0;p1<diagonal;p1++)
				{
					for(int m1 =0;m1<180;m1++)
					{
						 blackPtr = (byte*)(void*)blackScan0;
						blackPtr[(int)((p1*stride)/diagonal)
							+(int)((m1*bmp.Width*3 )/180)] += (byte)(accumulator[p1,m1].Count*50) ;
						blackPtr[(int)((p1*stride)/diagonal)
							+(int)((m1*bmp.Width*3 )/180)+1] += (byte)(accumulator[p1,m1].Count*50) ;
						blackPtr[(int)((p1*stride)/diagonal)
							+(int)((m1*bmp.Width*3 )/180)+2] +=(byte) (accumulator[p1,m1].Count*50) ;
								  
					}
				}*/





                } // End of Part For Loop







            } // unsafe of black bmp

            /////////////////////////
            /// Feature Vector Calling////////////
            /// 

            windowXY[0] = backLineX1;
            windowXY[1] = backLineY1;
            windowXY[2] = backLineX2;
            windowXY[3] = backLineY2;
            /*for(int i=0;i<4;i++)
				GaitRecognition.listDisplay[i]=windowXY[i];*/

            //FeatureVector fv = new FeatureVector();
            //fv.vectorBuilding(lineEndsXY, windowXY);
            ///////////////////////////////////////
            /////////////////////
            bmp.UnlockBits(bmpData);
            blackBmp.UnlockBits(blackData);
            //picture3.Image = blackBmp;
            return blackBmp;
        }
        ///////////////////////////
        private int[] calculateXYHand(int _x, int _y, int p, int theata)
        {
            int p1 = 0;

            for (int y = finalBackLineY1 + 20; y <= finalBackLineY2; y++)
            {
                p1 = (int)((finalBackLineX1 * Math.Cos((Math.PI * theata) / 180) + y * Math.Sin((Math.PI * theata) / 180)));

                if (p == p1)
                {
                    _y = y;
                }
            }

            _x = finalBackLineX1;
            int[] pixel = { _x, _y };
            return pixel;

        }//////////End of Function


        private int[] calculateXYHorizontal(int _x, int _y, int p, int theata)
        {
            int p1 = 0;
            for (int x = winX1; x <= winX2; x++)
            {
                p1 = (int)((x * Math.Cos((Math.PI * theata) / 180) + _y * Math.Sin((Math.PI * theata) / 180)));
                if (p == p1)
                {
                    _x = x;
                }
            }

            int[] pixel = { _x, _y };
            return pixel;

        }//////////End of Function


        private void backBoneLine()
        {
            midStick = Math.Abs((backLineY1 + backLineY2) / 2);

            winX1 = middleLineX - 5;
            winY1 = middleLineY;
            winX2 = middleLineX + 5;
            winY2 = midStick;

            /*listBox12.Items.Add (winX1);
			listBox12.Items.Add (winY1);
			listBox12.Items.Add (winX2);
			listBox12.Items.Add (winY2);*/

        }//////////End of Function

        private void rightHandLine()
        {

            winX1 = finalBackLineX2 + 10;
            winY1 = backLineY1 - 15;
            winX2 = backLineX2;
            winY2 = finalBackLineY2 - 3;

            /*listBox12.Items.Add (winX1);
			listBox12.Items.Add (winY1);
			listBox12.Items.Add (winX2);
			listBox12.Items.Add (winY2);*/

        }//////////End of Function

        private void leftHandLine()
        {
            winX1 = backLineX1;
            winY1 = backLineY1 - 15;
            winX2 = finalBackLineX1 - 10;
            winY2 = finalBackLineY2 - 5;

            /*listBox12.Items.Add (winX1);
			listBox12.Items.Add (winY1);
			listBox12.Items.Add (winX2);
			listBox12.Items.Add (winY2);*/

        }//////////End of Function

        private void rightLegLine()
        {
            winX1 = finalBackLineX2;
            winY1 = finalBackLineY2 + 1;
            winX2 = backLineX2;
            winY2 = backLineY2;

            /*listBox12.Items.Add (winX1);
			listBox12.Items.Add (winY1);
			listBox12.Items.Add (winX2);
			listBox12.Items.Add (winY2);*/

        }//////////End of Function

        private void secondRightLegLine()
        {
            winX1 = finalBackLineX1 + 3;
            winY1 = secondLeg;
            winX2 = backLineX2;
            winY2 = backLineY2;

            /*listBox12.Items.Add (winX1);
			listBox12.Items.Add (winY1);
			listBox12.Items.Add (winX2);
			listBox12.Items.Add (winY2);*/

        }//////////End of Function

        private void leftLegLine()
        {
            winX1 = backLineX1;
            winY1 = finalBackLineY2 + 1;
            winX2 = finalBackLineX2;
            winY2 = backLineY2;

            /*listBox12.Items.Add (winX1);
			listBox12.Items.Add (winY1);
			listBox12.Items.Add (winX2);
			listBox12.Items.Add (winY2);*/

        }//////////End of Function

        private void secondLeftLegLine()
        {
            winX1 = backLineX1;
            winY1 = secondLeg;
            winX2 = finalBackLineX1 - 3;
            winY2 = backLineY2;

            /*listBox12.Items.Add (winX1);
			listBox12.Items.Add (winY1);
			listBox12.Items.Add (winX2);
			listBox12.Items.Add (winY2);*/

        }//////////End of Function

        public void Histogram(Bitmap bmp)
        {

            Bitmap hist = (Bitmap)bmp.Clone();
            BitmapData bmData = hist.LockBits(new Rectangle(0, 0, hist.Width, hist.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            BitmapData bmSrc = bmp.LockBits(new Rectangle(0, 0, hist.Width, hist.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;
            System.IntPtr Scan1 = bmSrc.Scan0;
            int[] histogram1 = new int[256];
            int[] histogramX = new int[256];
            unsafe
            {
                byte* ptr = (byte*)(void*)Scan1;
                byte* p = (byte*)(void*)Scan1;

                int nOffset = stride - hist.Width * 3;
                int nWidth = hist.Width;
                int nHeight = hist.Height;
                float total_no_of_pixels = nWidth * nHeight;

                double[] histogram3 = new double[256];
                //int x1=-1,x2=257,y1=-1,y2=257;
                for (int i = 0; i < 256; i++)
                {
                    histogram1[i] = 0;
                    histogramX[i] = 0;
                    histogram3[i] = 0;
                }
                int count = 0;
                int xx = 0, yy = 0;
                for (int y = 0; y < nHeight; y++)
                {
                    for (int x = 0; x < nWidth; x++)
                    {
                        if (ptr[0] == 255 && count == 0)
                        {
                            histogram1[y] = histogram1[y] + 1;
                            count++;
                            xx = x;
                            yy = y;
                            //listBox18.Items.Add (xx);
                            //listBox18.Items.Add (yy);
                        }

                        if (ptr[0] == 255 && count > 0)
                        {
                            histogram1[y] = histogram1[y] + 1;
                        }
                        ptr += 3;
                    }
                    ptr += nOffset;
                }

                middleLineX = xx;
                middleLineY = yy;
                //listBox18.Items.Add (middleLineX);
                //listBox18.Items.Add (middleLineY);



                for (int y = 0; y < nHeight; y++)
                {
                    for (int x = 0; x < nWidth; x++)
                    {
                        if (p[0] == 255)
                            histogramX[x] = histogramX[x] + 1;
                        p += 3;
                    }
                    p += nOffset;
                }
            }

            int cy1 = 0, cy2 = 0, y1 = -1, y2 = 257, cx1 = 0, cx2 = 0, x1 = -1, x2 = 257;
            for (int y = 0; y < 256; y++)
            {
                if ((y1 < y) && (histogram1[y] > 0) && (cy1 == 0))
                {
                    y1 = y;
                    backLineY1 = y;
                    winY1 = y;
                    //listBox10.Items.Add (y);
                    cy1++;
                }

                if (x1 < y && histogramX[y] > 0 && cx1 == 0)
                {
                    x1 = y;
                    backLineX1 = x1;
                    winX1 = x1;
                    //listBox10.Items.Add (x1);
                    cx1++;
                }

            }

            for (int i = 255; i >= 0; i--)
            {
                if (y2 > i && histogram1[i] > 0 && cy2 == 0)
                {
                    y2 = i;
                    backLineY2 = y2;
                    winY2 = y2;
                    //listBox10.Items.Add (y2);
                    cy2++;
                }
                if (x2 > i && histogramX[i] > 0 && cx2 == 0)
                {
                    x2 = i;
                    backLineX2 = x2;
                    winX2 = x2;
                    //listBox10.Items.Add (x2);
                    cx2++;
                }
            }
            hist.UnlockBits(bmData);

        }//////////////////End of Function

    }//////////End Of Class
}
