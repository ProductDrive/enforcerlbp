using System;
using System.Text.RegularExpressions;

namespace enforcerWeb.Helper
{
    public class OTPGenerator
    {
        public static (string hash, string otp) GetOTPHash(string ans)
        {
            var answer = ans.ToUpper();
            int[] keyPositions = new int[answer.Length];
            char[] PkeyLetters = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J' };
            if (Regex.IsMatch(answer, @"^[a-jA-J]+$"))
            {
                var Pkey = answer.ToCharArray();

                for (var i = 0; i < Pkey.Length; i++)
                {
                    keyPositions[i] = Array.IndexOf(PkeyLetters, Pkey[i]);
                }
            
            }
            else
            {
                //Console.WriteLine("Enter your position key from letter A - J");
                return (null, null);
            }

            int[] otp = GetOTP(answer);
            



            //match otp keys
            char[] otpString = new char[otp.Length];
            char[] productD = new char[] { 'P', 'R', 'O', 'D', 'U', 'C', 'T','I', 'V','E' };
            for (var i = 0; i < otp.Length; i++)
            {
                otpString[i] = productD[otp[i]];

            }
            //return otpString;

            var guid = Guid.NewGuid().ToString("N");
            Console.WriteLine(guid);
            char[] newGuid = new char[guid.Length];
            for (var i = 0; i < guid.Length; i++)
            {
                newGuid[i] = guid[i];

            }

            //return newGuid;
            for (var i = 0; i < keyPositions.Length; i++)
            {
                var pos = keyPositions[i];
                var otpDigitString = otpString[i];
                newGuid[pos] = otpDigitString;
               
            }

            var result = String.Empty;
            if (otp[0] % 2 == 0)
            {
                result = String.Concat(String.Join("", newGuid).ToUpper(), ".", answer);


            }
            else
            {
                result = String.Concat(String.Join("", newGuid).ToLower(), ".", answer);

            }
            return (result, string.Join("", Array.ConvertAll(otp, delegate (int s) { return Convert.ToString(s); })));
        }

        public static int[] GetOTP(string answer)
        {
            //generate otp
            int[] otp = new int[answer.Length];
            for (var i = 0; i < answer.Length; i++)
            {
                Random random = new Random();
                otp[i] = random.Next(0, 9);
            }

            return otp;
        }
    }
}
