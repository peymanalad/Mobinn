using Abp.Authorization.Users;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto;

namespace Chamran.Deed.Authorization.Accounts.Validation
{
    public class StrongPasswordAttribute : ValidationAttribute
    {
        private readonly RSA _privateKey;

        public StrongPasswordAttribute()
        {
            string privateKeyInput = Environment.GetEnvironmentVariable("PRIVATE_KEY_PATH");
            if (string.IsNullOrWhiteSpace(privateKeyInput))
            {
                throw new InvalidOperationException("مسیر یا مقدار کلید خصوصی تنظیم نشده است.");
            }

            _privateKey = LoadPrivateKey(privateKeyInput);
        }

        private RSA LoadPrivateKey(string privateKeyInput)
        {

            string privateKeyPem = System.IO.File.Exists(privateKeyInput)
                ? System.IO.File.ReadAllText(privateKeyInput)
                : privateKeyInput;

            return ImportPrivateKeyFromPem(privateKeyPem);
        }

        private RSA ImportPrivateKeyFromPem(string privateKeyPem)
        {
            using (var reader = new StringReader(privateKeyPem))
            {
                var pemReader = new PemReader(reader);
                var keyPair = pemReader.ReadObject();

                if (keyPair is AsymmetricCipherKeyPair asymmetricKeyPair)
                {
                    var privateKeyParams = (RsaPrivateCrtKeyParameters)asymmetricKeyPair.Private;
                    var rsa = RSA.Create();
                    rsa.ImportParameters(DotNetUtilities.ToRSAParameters(privateKeyParams));
                    return rsa;
                }
                else if (keyPair is RsaPrivateCrtKeyParameters privateKeyParams)
                {
                    var rsa = RSA.Create();
                    rsa.ImportParameters(DotNetUtilities.ToRSAParameters(privateKeyParams));
                    return rsa;
                }
                else
                {
                    throw new InvalidOperationException("کلید خصوصی معتبر نیست.");
                }
            }
        }


        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            //if (value == null)
            //{
            //    return new ValidationResult("گذرواژه نمی‌تواند خالی باشد.");
            //}
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success;
            }

            string encryptedPassword = value.ToString();
            string password;

            try
            {
                password = DecryptPassword(encryptedPassword);
            }
            catch (Exception)
            {
                return new ValidationResult("فرمت گذرواژه رمزگذاری شده نامعتبر است.");
            }

            if (!IsPasswordStrong(password))
            {
                return new ValidationResult("گذرواژه باید شامل حروف کوچک، بزرگ، عدد و یک کاراکتر خاص باشد و حداقل 8 کاراکتر باشد.");
            }

            return ValidationResult.Success;
        }

        private bool IsPasswordStrong(string password)
        {
            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$";
            return password.Length >= 8 && password.Length <= AbpUserBase.MaxPlainPasswordLength && Regex.IsMatch(password, pattern);
        }

        private string DecryptPassword(string encryptedPassword)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedPassword);
            byte[] decryptedBytes = _privateKey.Decrypt(encryptedBytes, RSAEncryptionPadding.Pkcs1);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}
