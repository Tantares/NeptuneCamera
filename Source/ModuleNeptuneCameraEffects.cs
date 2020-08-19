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

        public static Texture2D GetTritanopiaTexture(Texture2D inputTexture)
        {
            // Change colour channels to mimic tritanopia.

            for (int y = 0; y < inputTexture.height; y++)
            {
                for (int x = 0; x < inputTexture.width; x++)
                {
                    var currentPixel = inputTexture.GetPixel(x, y);
                    var newPixel = new Color(currentPixel.r, currentPixel.g, currentPixel.g);
                    inputTexture.SetPixel(x, y, newPixel);
                }
            }

            return inputTexture;
        }

        public static Texture2D GetProtanopiaTexture(Texture2D inputTexture)
        {
            // Change colour channels to mimic protanopia.

            for (int y = 0; y < inputTexture.height; y++)
            {
                for (int x = 0; x < inputTexture.width; x++)
                {
                    var currentPixel = inputTexture.GetPixel(x, y);
                    var newPixel = new Color(currentPixel.g, currentPixel.g, currentPixel.b);
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
                        var pixel = inputTexture.GetPixel(x, y);
                        float newR = pixel.r * ((randomErrorChance % 2 == 0) ? 0.1f : 2.0f);
                        float newG = pixel.r * ((randomErrorChance % 2 == 0) ? 0.1f : 2.0f);
                        float newB = pixel.r * ((randomErrorChance % 2 == 0) ? 0.1f : 2.0f);

                        newR = Math.Min(1.0f, newR);
                        newG = Math.Min(1.0f, newG);
                        newB = Math.Min(1.0f, newB);

                        pixel.r = newR;
                        pixel.g = newG;
                        pixel.b = newB;

                        inputTexture.SetPixel(x, y, pixel);

                        // Old style error.
                        //inputTexture.SetPixel(x, y, (randomErrorChance % 2 == 0) ? Color.white : Color.black);
                    }
                }
            }

            return inputTexture;
        }

        public static Texture2D GetNoisyTexture(Texture2D inputTexture, int maxNoiseStrength)
        {
            // Randomly add noise to the input texture, up
            // to a given maximum noise strength.

            for (int y = 0; y < inputTexture.height; y++)
            {
                for (int x = 0; x < inputTexture.width; x++)
                {
                    int noiseStrength = randomNumberGenerator.Next(maxNoiseStrength);
                    int darkenOrLightenChance = randomNumberGenerator.Next(100);

                    float noiseMultiplier = noiseStrength / 100f;
                    
                    var pixel = inputTexture.GetPixel(x, y);
                    pixel = Color.Lerp(pixel, (darkenOrLightenChance % 2 == 0) ? Color.black : Color.white, noiseMultiplier);

                    inputTexture.SetPixel(x, y, pixel);
                }
            }
            
            return inputTexture;
        }
    }
}
