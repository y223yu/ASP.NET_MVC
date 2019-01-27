using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using YYClassLibrary;

namespace YYSail.Models
{
    public class YYMemberMetadata
    {
        public int MemberId { get; set; }

        [Display(Name = "Member")]
        public string FullName { get; set; }

        [Required]
        [Display(Name = "First Name")]
        //[Remote("YYCapitalize", "YYValidations")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Spouse First Name")]
        [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "[NULL]")]
        public string SpouseFirstName { get; set; }

        [Display(Name = "Spouse Last Name")]
        [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "[NULL]")]
        public string SpouseLastName { get; set; }

        public string Street { get; set; }
        public string City { get; set; }

        [Display(Name = "Province Name")]
        [StringLength(2)]
        [Remote("ProvinceCodeInDatabase", "YYMembers")]
        public string ProvinceCode { get; set; }

        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }

        [Required]
        [Display(Name = "Home Phone")]
        public string HomePhone { get; set; }

        [Display(Name = "Email Address")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Please use valid email format: example@example.com")]
        public string Email { get; set; }

        [Display(Name = "Year Joined")]
        public int? YearJoined { get; set; }

        [Display(Name = "Comments")]
        public string Comment { get; set; }

        [Display(Name = "Task Exempt?")]
        public bool TaskExempt { get; set; }

        [Display(Name = "Use Canada Post?")]
        public bool UseCanadaPost { get; set; }
    }

    [ModelMetadataTypeAttribute(typeof(YYMemberMetadata))]
    public partial class Member : IValidatableObject
    {
        private readonly SailContext _context;
        //would usually come from constants file or appsettings
        private readonly string CanadaCode = "CA";
        private readonly string USCode = "US";

        /// <summary>
        /// new ValidationResult (errors) only will be shown after fulfilling [Required] annotations 
        /// </summary>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var province = new Province();
            //Ref: https://andrewlock.net/injecting-services-into-validationattributes-in-asp-net-core/
            var _context = (SailContext)validationContext.GetService(typeof(SailContext));

            //Capitalize & Trim: FirstName, LastName, SpouseFirstName, SpouseLastName 
            FirstName = YYValidations.YYCapitalize(FirstName.Trim());
            LastName = YYValidations.YYCapitalize(LastName.Trim());
            if(String.IsNullOrEmpty(SpouseFirstName))
            {
                SpouseFirstName = null;
            }
            else
            {
                SpouseFirstName = YYValidations.YYCapitalize(SpouseFirstName.Trim());
            }
            if (String.IsNullOrEmpty(SpouseLastName))
            {
                SpouseLastName = null;
            }
            else
            {
                SpouseLastName = YYValidations.YYCapitalize(SpouseLastName.Trim());
            }
            
            //Capitalize & Trim: Street
            if(String.IsNullOrEmpty(Street))
            {
                Street = null;
            }
            else
            {
                Street = YYValidations.YYCapitalize(Street.Trim());
            }
            //Capitalize & Trim: City
            if (String.IsNullOrEmpty(City))
            {
                City = null;
            }
            else
            {
                City = YYValidations.YYCapitalize(City.Trim());
            }

            //Generate: FullName
            if (String.IsNullOrEmpty(SpouseFirstName) && String.IsNullOrEmpty(SpouseLastName))
            {
                FullName = LastName + ", " + FirstName;
            }
            else if (!String.IsNullOrEmpty(SpouseFirstName))
            {
                if (String.IsNullOrEmpty(SpouseLastName) || SpouseLastName == LastName)
                {
                    FullName = LastName + ", " + FirstName + " & " + SpouseFirstName;
                }
                else if (!String.IsNullOrEmpty(SpouseLastName))
                {
                    FullName = LastName + ", " + FirstName + " & " + SpouseLastName + ", " + SpouseFirstName;
                }
            }

            //Validate & Trim & ToUpper: ProvinceCode
            if (!String.IsNullOrEmpty(ProvinceCode))
            {
                ProvinceCode = ProvinceCode.Trim().ToUpper();
            }

            //Validate: PostalCode
            if (String.IsNullOrEmpty(PostalCode))
            {
                PostalCode = null;
            }
            else if (!String.IsNullOrEmpty(PostalCode))
            {
                PostalCode = PostalCode.Trim();
                //Re-format Canadian Postal Code
                if (YYValidations.YYPostalCodeValidation(PostalCode))
                {
                    PostalCode = YYValidations.YYPostalCodeFormat(PostalCode);
                    //Question: can _context be used in Models?
                    //List<string> provinceCanada = new List<string>() { "AB", "BC", "MB", "NB", "NL", "NS", "NT", "NU", "ON", "PE", "QC", "SK", "YT" };
                    //for (int i = 0; i < provinceCanada.Count; i++)
                    //{
                    //    if (ProvinceCode == provinceCanada[i])
                    //    {
                    //        PostalCode = YYValidations.YYPostalCodeFormat(PostalCode);
                    //    }
                    //}
                    //yield return new ValidationResult("Please use standard US(12345 or 12345-1234) postal code format",
                    //                                            new[] { nameof(PostalCode) });
                }
                //Re-format US Zip Code
                else
                {
                    string inputPostalCode = ""; //re inputPostalCode will be re-formated, then return to PostalCode

                    inputPostalCode = PostalCode;
                    if (YYValidations.YYZipCodeValidation(ref inputPostalCode))
                    {
                        PostalCode = inputPostalCode;
                    }
                    else
                    {
                        yield return new ValidationResult("Please use standard Canada(N2G 4M4)/US(12345 or 12345-1234) postal code format",
                                                                new[] { nameof(PostalCode) });
                    }
                }
                //ProvinceCode cannot be empty if has PostalCode/ZipCode
                if(String.IsNullOrEmpty(ProvinceCode))
                {
                    yield return new ValidationResult("Please offer province code that is needed by postal code",
                                                                    new[] { nameof(ProvinceCode) });
                }
            }

            //Trim & Re-format: HomePhone
            HomePhone = YYValidations.YYExtractDigits(HomePhone.Trim());
            if (HomePhone.Length != 10)
            {
                yield return new ValidationResult("The home phone must only containt 10 digits",
                                                                    new[] { nameof(HomePhone) });
            }
            else
            {
                HomePhone = HomePhone.Insert(3, "-").Insert(7, "-");
            }

            //Trim: Email
            if (String.IsNullOrEmpty(Email))
            {
                Email = null;
            }
            else
            {
                Email = Email.Trim();
            }
            
            //Trim: Comment
            if(String.IsNullOrEmpty(Comment))
            {
                Comment = null;
            }
            else
            {
                Comment = Comment.Trim();
            }

            //Validate: YearJoined
            if(YearJoined.HasValue)
            {
                if (YearJoined > DateTime.Now.Year)
                {
                    YearJoined = null;
                    yield return new ValidationResult("The year cannot be in future", new[] { nameof(YearJoined) });
                }
            }

            //Validate: useCanadaPost
            //1. do not use CanadaPost
            if (UseCanadaPost == false && String.IsNullOrEmpty(Email))
            {
                yield return new ValidationResult("Please offer an Email if no using Canada Post.", new[] { nameof(Email) });
            }
            //2. use CanadaPost
            else if(UseCanadaPost == true)
            {
                if(String.IsNullOrEmpty(Street))
                {
                    yield return new ValidationResult("Please offer street address if using Canada Post.", new[] { nameof(Street) });
                }
                if (String.IsNullOrEmpty(City))
                {
                    yield return new ValidationResult("Please offer city name if using Canada Post.", new[] { nameof(City) });
                }
                if (String.IsNullOrEmpty(ProvinceCode))
                {
                    yield return new ValidationResult("Please offer province if using Canada Post.", new[] { nameof(ProvinceCode) });
                }
                if (String.IsNullOrEmpty(PostalCode))
                {
                    yield return new ValidationResult("Please offer postal code if using Canada Post.", new[] { nameof(PostalCode) });
                }
            }

            yield return ValidationResult.Success;
        }
    }
}
