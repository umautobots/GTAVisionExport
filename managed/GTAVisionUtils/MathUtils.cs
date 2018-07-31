using System;
using System.Collections.Generic;

namespace GTAVisionUtils {
    public class MathUtils {
        public static List<double> cumsum(List<double> arr) {
            var cumsumArr = new List<double>(arr.Count);
            double sum = 0;
            for (int i = 0; i < arr.Count; i++) {
                sum += arr[i];
                cumsumArr[i] = sum;
            }

            return cumsumArr;
        }

        public static List<float> cumsum(List<float> arr) {
            var cumsumArr = new List<float>(arr.Count);
            float sum = 0;
            for (int i = 0; i < arr.Count; i++) {
                sum += arr[i];
                cumsumArr[i] = sum;
            }

            return cumsumArr;
        }

        public static List<int> cumsum(List<int> arr) {
            var cumsumArr = new List<int>(arr.Count);
            int sum = 0;
            for (int i = 0; i < arr.Count; i++) {
                sum += arr[i];
                cumsumArr.Add(sum);
            }

            return cumsumArr;
        }

        /// <summary>
        /// Returns index of bin from cumulative sum array, same functionality as in numpy.digitize
        /// </summary>
        /// <returns></returns>
        public static int digitize(int number, List<int> cumsumArr) {
            for (int i = 0; i < cumsumArr.Count; i++) {
                if (number < cumsumArr[i]) {
                    return i;
                }
            }

            throw new Exception("number is out of range of cumcum array");
        }
        
    }
}