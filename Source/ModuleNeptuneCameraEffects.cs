using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using UnityEngine.Rendering;

namespace NeptuneCamera
{
    public class ModuleNeptuneCameraEffects
    {
        public static System.Random randomNumberGenerator = new System.Random();

        public static Texture2D GetRedTexture(Texture2D inputTexture)
        {
            // Change all pixels to their red
            // component only.

            for (int y = 0; y < inputTexture.height; y++)
            {
                for (int x = 0; x < inputTexture.width; x++)
                {
                    var currentPixel = inputTexture.GetPixel(x, y);
                    var newPixel = new Color(currentPixel.r, 0, 0);
                    inputTexture.SetPixel(x, y, newPixel);
                }
            }

            return inputTexture;
        }

        public static Texture2D GetGreenTexture(Texture2D inputTexture)
        {
            // Change all pixels to their green
            // component only.

            for (int y = 0; y < inputTexture.height; y++)
            {
                for (int x = 0; x < inputTexture.width; x++)
                {
                    var currentPixel = inputTexture.GetPixel(x, y);
                    var newPixel = new Color(0, currentPixel.g, 0);
                    inputTexture.SetPixel(x, y, newPixel);
                }
            }

            return inputTexture;
        }

        public static Texture2D GetBlueTexture(Texture2D inputTexture)
        {
            // Change all pixels to their blue
            // component only.

            for (int y = 0; y < inputTexture.height; y++)
            {
                for (int x = 0; x < inputTexture.width; x++)
                {
                    var currentPixel = inputTexture.GetPixel(x, y);
                    var newPixel = new Color(0, 0, currentPixel.b);
                    inputTexture.SetPixel(x, y, newPixel);
                }
            }

            return inputTexture;
        }

        public static Texture2D GetGreyscaleTexture(Texture2D inputTexture)
        {
            // Change all pixels to their greyscale
            // component only.

            for (int y = 0; y < inputTexture.height; y++)
            {
                for (int x = 0; x < inputTexture.width; x++)
                {
                    var currentPixel = inputTexture.GetPixel(x, y);
                    var newPixel = new Color
                        (currentPixel.grayscale, currentPixel.grayscale, currentPixel.grayscale);
                    inputTexture.SetPixel(x, y, newPixel);
                }
            }

            return inputTexture;
        }

        public static Texture2D GetErrorDamagedTexture(Texture2D inputTexture, int errorRate)
        {
            // Randomly scramble the texture with black
            // and white pixels, based on the error rate.

            for (int y = 0; y < inputTexture.height; y++)
            {
                for (int x = 0; x < inputTexture.width; x++)
                {
                    int randomErrorChance = randomNumberGenerator.Next(100);

                    if (randomErrorChance <= errorRate)
                    {
                        inputTexture.SetPixel(x, y, (randomErrorChance % 2 == 0) ? Color.white : Color.black);
                    }
                }
            }

            return inputTexture;
        }
    }
}
