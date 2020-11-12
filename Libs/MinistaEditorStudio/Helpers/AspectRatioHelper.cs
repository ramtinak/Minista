using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Minista.Helpers
{
    public static class AspectRatioHelper
    {
        public static double GetAspectRatioForMedia(double Width, double Height)
        { 
            return Width / Height;
        }


        /// <summary>
        ///     ali ngame code
        /// </summary>
        public static Size CalculateSizeInBox(double imageWidth, double imageHeight, double windowHeight, double windowWidth)
        {
            double dbl = imageWidth / imageHeight;
            ////if (windowHeight == 0 && windowWidth == 0)
            ////    return new Size(imageHeight / dbl, (imageWidth / dbl));
            //calculate the ratio

            //set height of image to boxHeight and check if resulting width is less than boxWidth, 
            //else set width of image to boxWidth and calculate new height
            if (windowHeight * dbl <= windowWidth)
                return new Size((windowHeight * dbl), windowHeight);
            else
                return new Size(windowWidth, (windowWidth / dbl));
        }
        private static Size GetSizeAdjustedToAspectRatio(double sourceWidth, double sourceHeight, double dWidth, double dHeight)
        {
            bool isLandscape = sourceWidth > sourceHeight;

            double newHeight;
            double newWidth;
            if (isLandscape)
            {
                newHeight = dWidth * sourceHeight / sourceWidth;
                newWidth = dWidth;
            }
            else
            {
                newWidth = dHeight * sourceWidth / sourceHeight;
                newHeight = dHeight;
            }

            return new Size(newWidth, newHeight);
        }

        public static Size GetAspectRatioX2(double sourceWidth, double sourceHeight/*, double dWidth = 1080, double dHeight = 1920*/)
        {
            var size = GetDesireSize(sourceWidth, sourceHeight, true);
            //bool isLandscape = sourceWidth > sourceHeight;

            //double dWidth = size.Width;
            //double dHeight = size.Height;


            //double newHeight;
            //double newWidth;
            //if (isLandscape)
            //{
            //    newHeight = dWidth * sourceHeight / sourceWidth;
            //    newWidth = dWidth;
            //}
            //else
            //{
            //    newWidth = dHeight * sourceWidth / sourceHeight;
            //    newHeight = dHeight;
            //}
            //    return new Size((int)newWidth, (int)newHeight);
            return size;
        }
        public static Size GetAspectRatioX(double sourceWidth, double sourceHeight, double dWidth = 1080, double dHeight = 1920)
        {
            bool isLandscape = sourceWidth > sourceHeight;

            double newHeight;
            double newWidth;
            if (isLandscape)
            {
                newHeight = dWidth * sourceHeight / sourceWidth;
                newWidth = dWidth;
            }
            else
            {
                newWidth = dHeight * sourceWidth / sourceHeight;
                newHeight = dHeight;
            }
            if (newHeight > 1920 || newWidth > 1920)
            {
                var size = GetAspectRatio(sourceWidth, sourceHeight);
                if (size.Width > size.Height)
                    return new Size(size.Height, size.Width);
                else
                    return new Size(size.Width, size.Height);
            }
            if (newWidth > newHeight)
                return new Size(newHeight, newWidth);
            else
                return new Size(newWidth, newHeight);
        }
        public static Size GetAspectRatio(double sourceWidth, double sourceHeight)
        {
            var size = GetDesireSize(sourceWidth, sourceHeight/*, true*/);
            bool isLandscape = sourceWidth > sourceHeight;

            double dWidth = size.Width;
            double dHeight = size.Height;


            double newHeight;
            double newWidth;
            if (isLandscape)
            {
                newHeight = dWidth * sourceHeight / sourceWidth;
                newWidth = dWidth;
            }
            else
            {
                newWidth = dHeight * sourceWidth / sourceHeight;
                newHeight = dHeight;
            }
            if (newWidth > newHeight)
                return new Size((int)newHeight, (int)newWidth);
            else
                return new Size((int)newWidth, (int)newHeight);
            //return new Size(newWidth, newHeight);
        }
        public static Size GetDesireSize(double sourceWidth, double sourceHeight, bool isLandscape = false)
        {
            //Instagram story dimensions: 1080px wide by 1920 px in height
            //This is a 9:16 ratio
            //720px wide by 1280px in height will give you an HD photo or video with a slightly faster upload time
            //450px wide by 800px in height (or smaller) will give you a faster upload time with a lower post quality

            double newHeight;
            double newWidth;
            if (sourceWidth == sourceHeight)
            {
                if (sourceWidth >= 1080)
                {
                    newHeight = 1080;
                    newWidth = 1080;
                }
                else if (sourceWidth < 1080 && sourceWidth >= 720)
                {
                    newHeight = 1080;
                    newWidth = 1080;
                }
                else
                {
                    newHeight = 800;
                    newWidth = 800;
                }
            }
            else if (sourceWidth > sourceHeight)
            {
                if (sourceWidth >= 1080)
                {
                    newHeight = 1080;
                    newWidth = 1920;
                }
                else if (sourceWidth < 1080 && sourceWidth >= 720)
                {
                    newHeight = 720;
                    newWidth = 1280;
                }
                else
                {
                    newHeight = 450;
                    newWidth = 800;
                }
            }
            else
            {

                if (sourceWidth >= 1080)
                {
                    newHeight = 1920;
                    newWidth = 1080;
                }
                else if (sourceWidth < 1080 && sourceWidth >= 720)
                {
                    newHeight = 1280;
                    newWidth = 720;
                }
                else
                {
                    newHeight = 800;
                    newWidth = 450;
                }
            }
            return new Size(newWidth, newHeight);
        }
    }
}
