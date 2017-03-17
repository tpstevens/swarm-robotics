using UnityEngine;

namespace Utilities
{
    public abstract class Letter
    {
        public abstract Vector2[] getRow(int row); // row is numbered from 0 = top, 6 = bottom

        protected static Vector2[][] generatePixelCoordinates(int[][] rowPositions)
        {
            Vector2[][] pixelCoordinates = new Vector2[7][];

            for (int i = 0; i < 7; ++i)
            {
                pixelCoordinates[i] = new Vector2[rowPositions[i].Length];
                for (int j = 0; j < rowPositions[i].Length; ++j)
                {
                    pixelCoordinates[i][j] = new Vector2(rowPositions[i][j], 6 - i);
                }
            }

            return pixelCoordinates;
        }
    }

    class LetterA : Letter
    {
        private static int[][] rowPositions = new int[][] {
            new int[] { 1, 2, 3 },
            new int[] { 0, 4 },
            new int[] { 0, 4 },
            new int[] { 0, 1, 2, 3, 4 },
            new int[] { 0, 4 },
            new int[] { 0, 4 },
            new int[] { 0, 4 }
        };

        private static Vector2[][] pixelCoordinates;

        static LetterA() {
            pixelCoordinates = generatePixelCoordinates(rowPositions);
        }

        public override Vector2[] getRow(int row) {
            return pixelCoordinates[row];
        }
    }

    class LetterB : Letter
    {
        private static int[][] rowPositions = new int[][] {
            new int[] { 0, 1, 2, 3 },
            new int[] { 0, 4 },
            new int[] { 0, 4 },
            new int[] { 0, 1, 2, 3 },
            new int[] { 0, 4 },
            new int[] { 0, 4 },
            new int[] { 0, 1, 2, 3 }
        };

        private static Vector2[][] pixelCoordinates;

        static LetterB() {
            pixelCoordinates = generatePixelCoordinates(rowPositions);
        }

        public override Vector2[] getRow(int row) {
            return pixelCoordinates[row];
        }
    }

    class LetterC : Letter
    {
        private static int[][] rowPositions = new int[][] {
            new int[] { 1, 2, 3 },
            new int[] { 0, 4 },
            new int[] { 0 },
            new int[] { 0 },
            new int[] { 0 },
            new int[] { 0, 4 },
            new int[] { 1, 2, 3 }
        };

        private static Vector2[][] pixelCoordinates;

        static LetterC()
        {
            pixelCoordinates = generatePixelCoordinates(rowPositions);
        }

        public override Vector2[] getRow(int row)
        {
            return pixelCoordinates[row];
        }
    }

    class LetterD : Letter
    {
        private static int[][] rowPositions = new int[][] {
            new int[] { 0, 1, 2, 3 },
            new int[] { 0, 4 },
            new int[] { 0, 4 },
            new int[] { 0, 4 },
            new int[] { 0, 4 },
            new int[] { 0, 4 },
            new int[] { 0, 1, 2, 3 }
        };

        private static Vector2[][] pixelCoordinates;

        static LetterD()
        {
            pixelCoordinates = generatePixelCoordinates(rowPositions);
        }

        public override Vector2[] getRow(int row)
        {
            return pixelCoordinates[row];
        }
    }

    class LetterE : Letter
    {
        private static int[][] rowPositions = new int[][] {
            new int[] { 0, 1, 2, 3, 4 },
            new int[] { 0 },
            new int[] { 0 },
            new int[] { 0, 1, 2, 3 },
            new int[] { 0 },
            new int[] { 0 },
            new int[] { 0, 1, 2, 3, 4 }
        };

        private static Vector2[][] pixelCoordinates;

        static LetterE()
        {
            pixelCoordinates = generatePixelCoordinates(rowPositions);
        }

        public override Vector2[] getRow(int row)
        {
            return pixelCoordinates[row];
        }
    }

    class LetterF : Letter
    {
        private static int[][] rowPositions = new int[][] {
            new int[] { 0, 1, 2, 3, 4 },
            new int[] { 0 },
            new int[] { 0 },
            new int[] { 0, 1, 2, 3 },
            new int[] { 0 },
            new int[] { 0 },
            new int[] { 0 }
        };

        private static Vector2[][] pixelCoordinates;

        static LetterF()
        {
            pixelCoordinates = generatePixelCoordinates(rowPositions);
        }

        public override Vector2[] getRow(int row)
        {
            return pixelCoordinates[row];
        }
    }

    class LetterG : Letter
    {
        private static int[][] rowPositions = new int[][] {
            new int[] { 1, 2, 3 },
            new int[] { 0, 4 },
            new int[] { 0 },
            new int[] { 0, 2, 3, 4 },
            new int[] { 0, 4 },
            new int[] { 0, 4 },
            new int[] { 1, 2, 3 }
        };

        private static Vector2[][] pixelCoordinates;

        static LetterG()
        {
            pixelCoordinates = generatePixelCoordinates(rowPositions);
        }

        public override Vector2[] getRow(int row)
        {
            return pixelCoordinates[row];
        }
    }

    class LetterH : Letter
    {
        private static int[][] rowPositions = new int[][] {
            new int[] { 0, 4 },
            new int[] { 0, 4 },
            new int[] { 0, 4 },
            new int[] { 0, 1, 2, 3, 4 },
            new int[] { 0, 4 },
            new int[] { 0, 4 },
            new int[] { 0, 4 }
        };

        private static Vector2[][] pixelCoordinates;

        static LetterH()
        {
            pixelCoordinates = generatePixelCoordinates(rowPositions);
        }

        public override Vector2[] getRow(int row)
        {
            return pixelCoordinates[row];
        }
    }

    class LetterI : Letter
    {
        private static int[][] rowPositions = new int[][] {
            new int[] { 0, 1, 2, 3, 4 },
            new int[] { 2 },
            new int[] { 2 },
            new int[] { 2 },
            new int[] { 2 },
            new int[] { 2 },
            new int[] { 0, 1, 2, 3, 4 }
        };

        private static Vector2[][] pixelCoordinates;

        static LetterI()
        {
            pixelCoordinates = generatePixelCoordinates(rowPositions);
        }

        public override Vector2[] getRow(int row)
        {
            return pixelCoordinates[row];
        }
    }

    class LetterJ : Letter
    {
        private static int[][] rowPositions = new int[][] {
            new int[] { 1, 2, 3, 4 },
            new int[] { 4 },
            new int[] { 4 },
            new int[] { 4 },
            new int[] { 4 },
            new int[] { 0, 4 },
            new int[] { 1, 2, 3 }
        };

        private static Vector2[][] pixelCoordinates;

        static LetterJ()
        {
            pixelCoordinates = generatePixelCoordinates(rowPositions);
        }

        public override Vector2[] getRow(int row)
        {
            return pixelCoordinates[row];
        }
    }

    class LetterK : Letter
    {
        private static int[][] rowPositions = new int[][] {
            new int[] { 0, 4 },
            new int[] { 0, 3 },
            new int[] { 0, 2 },
            new int[] { 0, 1 },
            new int[] { 0, 2 },
            new int[] { 0, 3 },
            new int[] { 0, 4 }
        };

        private static Vector2[][] pixelCoordinates;

        static LetterK()
        {
            pixelCoordinates = generatePixelCoordinates(rowPositions);
        }

        public override Vector2[] getRow(int row)
        {
            return pixelCoordinates[row];
        }
    }

    class LetterL : Letter
    {
        private static int[][] rowPositions = new int[][] {
            new int[] { 0 },
            new int[] { 0 },
            new int[] { 0 },
            new int[] { 0 },
            new int[] { 0 },
            new int[] { 0 },
            new int[] { 0, 1, 2, 3, 4 }
        };

        private static Vector2[][] pixelCoordinates;

        static LetterL()
        {
            pixelCoordinates = generatePixelCoordinates(rowPositions);
        }

        public override Vector2[] getRow(int row)
        {
            return pixelCoordinates[row];
        }
    }

    class LetterM : Letter
    {
        private static int[][] rowPositions = new int[][] {
            new int[] { 0, 1, 3, 4 },
            new int[] { 0, 2, 4 },
            new int[] { 0, 2, 4 },
            new int[] { 0, 2, 4 },
            new int[] { 0, 4 },
            new int[] { 0, 4 },
            new int[] { 0, 4 }
        };

        private static Vector2[][] pixelCoordinates;

        static LetterM()
        {
            pixelCoordinates = generatePixelCoordinates(rowPositions);
        }

        public override Vector2[] getRow(int row)
        {
            return pixelCoordinates[row];
        }
    }

    class LetterN : Letter
    {
        private static int[][] rowPositions = new int[][] {
            new int[] { 0, 4 },
            new int[] { 0, 1, 4 },
            new int[] { 0, 1, 4 },
            new int[] { 0, 2, 4 },
            new int[] { 0, 3, 4 },
            new int[] { 0, 3, 4 },
            new int[] { 0, 4 }
        };

        private static Vector2[][] pixelCoordinates;

        static LetterN()
        {
            pixelCoordinates = generatePixelCoordinates(rowPositions);
        }

        public override Vector2[] getRow(int row)
        {
            return pixelCoordinates[row];
        }
    }

    class LetterO : Letter
    {
        private static int[][] rowPositions = new int[][] {
            new int[] { 1, 2, 3 },
            new int[] { 0, 4 },
            new int[] { 0, 4 },
            new int[] { 0, 4 },
            new int[] { 0, 4 },
            new int[] { 0, 4 },
            new int[] { 1, 2, 3 }
        };

        private static Vector2[][] pixelCoordinates;

        static LetterO()
        {
            pixelCoordinates = generatePixelCoordinates(rowPositions);
        }

        public override Vector2[] getRow(int row)
        {
            return pixelCoordinates[row];
        }
    }

    class LetterP : Letter
    {
        private static int[][] rowPositions = new int[][] {
            new int[] { 0, 1, 2, 3 },
            new int[] { 0, 4 },
            new int[] { 0, 4 },
            new int[] { 0, 1, 2, 3 },
            new int[] { 0 },
            new int[] { 0 },
            new int[] { 0 }
        };

        private static Vector2[][] pixelCoordinates;

        static LetterP()
        {
            pixelCoordinates = generatePixelCoordinates(rowPositions);
        }

        public override Vector2[] getRow(int row)
        {
            return pixelCoordinates[row];
        }
    }

    class LetterQ : Letter
    {
        private static int[][] rowPositions = new int[][] {
            new int[] { 1, 2, 3 },
            new int[] { 0, 4 },
            new int[] { 0, 4 },
            new int[] { 0, 4 },
            new int[] { 0, 2, 4 },
            new int[] { 0, 3 },
            new int[] { 1, 2, 4 }
        };

        private static Vector2[][] pixelCoordinates;

        static LetterQ()
        {
            pixelCoordinates = generatePixelCoordinates(rowPositions);
        }

        public override Vector2[] getRow(int row)
        {
            return pixelCoordinates[row];
        }
    }

    class LetterR : Letter
    {
        private static int[][] rowPositions = new int[][] {
            new int[] { 0, 1, 2, 3 },
            new int[] { 0, 4 },
            new int[] { 0, 4 },
            new int[] { 0, 1, 2, 3 },
            new int[] { 0, 2 },
            new int[] { 0, 3 },
            new int[] { 0, 4 }
        };

        private static Vector2[][] pixelCoordinates;

        static LetterR()
        {
            pixelCoordinates = generatePixelCoordinates(rowPositions);
        }

        public override Vector2[] getRow(int row)
        {
            return pixelCoordinates[row];
        }
    }

    class LetterS : Letter
    {
        private static int[][] rowPositions = new int[][] {
            new int[] { 1, 2, 3 },
            new int[] { 0, 4 },
            new int[] { 0 },
            new int[] { 1, 2, 3 },
            new int[] { 4 },
            new int[] { 0, 4 },
            new int[] { 1, 2, 3 }
        };

        private static Vector2[][] pixelCoordinates;

        static LetterS()
        {
            pixelCoordinates = generatePixelCoordinates(rowPositions);
        }

        public override Vector2[] getRow(int row)
        {
            return pixelCoordinates[row];
        }
    }

    class LetterT : Letter
    {
        private static int[][] rowPositions = new int[][] {
            new int[] { 0, 1, 2, 3, 4 },
            new int[] { 2 },
            new int[] { 2 },
            new int[] { 2 },
            new int[] { 2 },
            new int[] { 2 },
            new int[] { 2 }
        };

        private static Vector2[][] pixelCoordinates;

        static LetterT()
        {
            pixelCoordinates = generatePixelCoordinates(rowPositions);
        }

        public override Vector2[] getRow(int row)
        {
            return pixelCoordinates[row];
        }
    }

    class LetterU : Letter
    {
        private static int[][] rowPositions = new int[][] {
            new int[] { 0, 4 },
            new int[] { 0, 4 },
            new int[] { 0, 4 },
            new int[] { 0, 4 },
            new int[] { 0, 4 },
            new int[] { 0, 4 },
            new int[] { 1, 2, 3 }
        };

        private static Vector2[][] pixelCoordinates;

        static LetterU()
        {
            pixelCoordinates = generatePixelCoordinates(rowPositions);
        }

        public override Vector2[] getRow(int row)
        {
            return pixelCoordinates[row];
        }
    }

    class LetterV : Letter
    {
        private static int[][] rowPositions = new int[][] {
            new int[] { 0, 4 },
            new int[] { 0, 4 },
            new int[] { 0, 4 },
            new int[] { 0, 4 },
            new int[] { 0, 4 },
            new int[] { 1, 3 },
            new int[] { 2 }
        };

        private static Vector2[][] pixelCoordinates;

        static LetterV()
        {
            pixelCoordinates = generatePixelCoordinates(rowPositions);
        }

        public override Vector2[] getRow(int row)
        {
            return pixelCoordinates[row];
        }
    }

    class LetterW : Letter
    {
        private static int[][] rowPositions = new int[][] {
            new int[] { 0, 4 },
            new int[] { 0, 4 },
            new int[] { 0, 2, 4 },
            new int[] { 0, 2, 4 },
            new int[] { 0, 2, 4 },
            new int[] { 0, 2, 4 },
            new int[] { 1, 3 }
        };

        private static Vector2[][] pixelCoordinates;

        static LetterW()
        {
            pixelCoordinates = generatePixelCoordinates(rowPositions);
        }

        public override Vector2[] getRow(int row)
        {
            return pixelCoordinates[row];
        }
    }

    class LetterX : Letter
    {
        private static int[][] rowPositions = new int[][] {
            new int[] { 0, 4 },
            new int[] { 1, 3 },
            new int[] { 2 },
            new int[] { 2 },
            new int[] { 2 },
            new int[] { 1, 3 },
            new int[] { 0, 4 }
        };

        private static Vector2[][] pixelCoordinates;

        static LetterX()
        {
            pixelCoordinates = generatePixelCoordinates(rowPositions);
        }

        public override Vector2[] getRow(int row)
        {
            return pixelCoordinates[row];
        }
    }

    class LetterY : Letter
    {
        private static int[][] rowPositions = new int[][] {
            new int[] { 0, 4 },
            new int[] { 0, 4 },
            new int[] { 0, 4 },
            new int[] { 1, 3 },
            new int[] { 2 },
            new int[] { 2 },
            new int[] { 2 }
        };

        private static Vector2[][] pixelCoordinates;

        static LetterY()
        {
            pixelCoordinates = generatePixelCoordinates(rowPositions);
        }

        public override Vector2[] getRow(int row)
        {
            return pixelCoordinates[row];
        }
    }

    class LetterZ : Letter
    {
        private static int[][] rowPositions = new int[][] {
            new int[] { 0, 1, 2, 3, 4 },
            new int[] { 4 },
            new int[] { 3 },
            new int[] { 2 },
            new int[] { 1 },
            new int[] { 0 },
            new int[] { 0, 1, 2, 3, 4 }
        };

        private static Vector2[][] pixelCoordinates;

        static LetterZ()
        {
            pixelCoordinates = generatePixelCoordinates(rowPositions);
        }

        public override Vector2[] getRow(int row)
        {
            return pixelCoordinates[row];
        }
    }
}
