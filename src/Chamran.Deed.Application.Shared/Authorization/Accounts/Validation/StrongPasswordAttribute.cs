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
        private RSA _privateKey;
        private bool _keyLoadAttempted = false;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return ValidationResult.Success;

            if (!_keyLoadAttempted)
            {
                _keyLoadAttempted = true;
                try
                {
                    LoadPrivateKeyFromEnv();
                }
                catch
                {
                    // Ignore key loading error to allow Swagger and other non-auth flows to work
                }
            }

            if (_privateKey == null)
                return new ValidationResult("رمز عبور قابل بررسی نیست. کلید رمزگشایی در دسترس نیست.");

            string encryptedPassword = value.ToString();
            string password;

            try
            {
                password = DecryptPassword(encryptedPassword);
            }
            catch
            {
                return new ValidationResult("فرمت گذرواژه رمزگذاری شده نامعتبر است.");
            }

            if (!IsPasswordStrong(password))
            {
                return new ValidationResult("گذرواژه باید شامل حروف کوچک، بزرگ، عدد و یک کاراکتر خاص باشد و حداقل 8 کاراکتر باشد.");
            }

            return ValidationResult.Success;
        }

        private void LoadPrivateKeyFromEnv()
        {
            string privateKeyInput = Environment.GetEnvironmentVariable("PRIVATE_KEY_PATH");

            if (string.IsNullOrWhiteSpace(privateKeyInput))
                throw new InvalidOperationException("متغیر PRIVATE_KEY_PATH تنظیم نشده است.");

            string privateKeyPem = File.Exists(privateKeyInput)
                ? File.ReadAllText(privateKeyInput)
                : privateKeyInput;

            _privateKey = ImportPrivateKeyFromPem(privateKeyPem);
        }

        private RSA ImportPrivateKeyFromPem(string privateKeyPem)
        {
            using var reader = new StringReader(privateKeyPem);
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

            throw new InvalidOperationException("کلید خصوصی معتبر نیست.");
        }

        private string DecryptPassword(string encryptedPassword)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedPassword);
            byte[] decryptedBytes = _privateKey.Decrypt(encryptedBytes, RSAEncryptionPadding.Pkcs1);
            return Encoding.UTF8.GetString(decryptedBytes);
        }

        private bool IsPasswordStrong(string password)
        {
            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$";
            return password.Length >= 8 && password.Length <= AbpUserBase.MaxPlainPasswordLength && Regex.IsMatch(password, pattern);
        }
    }
}
