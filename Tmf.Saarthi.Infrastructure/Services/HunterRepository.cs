using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Tmf.Saarthi.Core.Options;
using Tmf.Saarthi.Core.RequestModels.Hunter;
using Tmf.Saarthi.Core.ResponseModels.Hunter;
using Tmf.Saarthi.Infrastructure.HttpService;
using Tmf.Saarthi.Infrastructure.Interfaces;
using Tmf.Saarthi.Infrastructure.SqlService;

namespace Tmf.Saarthi.Infrastructure.Services
{
    public class HunterRepository : IHunterRepository
    {

        private readonly IHttpService _httpService;
        private readonly ISqlUtility _sqlUtility;
        private readonly HunterOptions _hunterOptions;
        private readonly ConnectionStringsOptions _connectionStringsOptions;

        public HunterRepository(ISqlUtility sqlUtility, IHttpService httpService, IOptions<HunterOptions> hunterOptions, IOptions<ConnectionStringsOptions> connectionStringsOptions)
        {
            _httpService = httpService;
            _hunterOptions = hunterOptions.Value;
            _connectionStringsOptions = connectionStringsOptions.Value;
            _sqlUtility = sqlUtility;
        }

        public async Task<HunterResponseModel> HunterVerification(HunterRequestModel requestModel)
        {
           
            var jsonSerializerOptions = new JsonSerializerOptions() { WriteIndented = true };
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("BpNo", "1");
            headers.Add("UserType", "User");
         var t=   JsonSerializer.Serialize(requestModel);
            JsonDocument result = await _httpService.PostAsync(_hunterOptions.BaseUrl+ _hunterOptions.ValidateCustomer, requestModel, headers);
            HunterResponseModel hunterResponseModel = JsonSerializer.Deserialize<HunterResponseModel>(result, jsonSerializerOptions) ?? throw new ArgumentNullException();


            return hunterResponseModel;
        }


        public async Task<bool> HunterRequest()
        {    
            DataTable dt = await _sqlUtility.ExecuteCommandAsync(_connectionStringsOptions.DefaultConnection, "usp_getAllHunterPendinginfo");

            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    List<SqlParameter> parameters = new List<SqlParameter>()
                    {
                        new SqlParameter("BPnumber", Convert.ToInt32( dt.Rows[i]["BpNumber"])),
                        new SqlParameter("FleetId", Convert.ToInt32( dt.Rows[i]["FleetId"])),
                    };

                    DataTable dt1 = await _sqlUtility.ExecuteCommandAsync(_connectionStringsOptions.DefaultConnection, "usp_getHunterRequestParms", parameters);

                    HunterRequestModel requestModel = HunterRequestMapper(dt1, i);

                    HunterResponseModel hunterResponseModel = await HunterVerification(requestModel);
                }
            }
            return true;
            
        }

        public async Task AddHunterResponse(HunterResponseModel addHunterResponseModel)
        {
               if (addHunterResponseModel.ResponseHeader.Count > 0)
                {
                    foreach (var t in addHunterResponseModel.ResponseHeader) { 
                        List<SqlParameter> parameters = new List<SqlParameter>()
                            {
                                new SqlParameter("RequestType",t.ClientReferenceId),
                                new SqlParameter("RequestType",""),
                                new SqlParameter("RequestType",""),
                            };

                    await _sqlUtility.ExecuteCommandAsync(_connectionStringsOptions.DefaultConnection, "usp_addHunterResponse", parameters);
                }
            }
        }


        private static HunterRequestModel HunterRequestMapper(DataTable dt, int i)
        {
            HunterRequestModel requestModel = new HunterRequestModel();

            //************Header Request************//
            Header header = new Header();
            header.MessageTime = DateTime.Now;
            header.ClientReferenceId = dt.Rows[i]["BPNumber"].ToString();
            header.TenantId = "3987486471cd4376809b3f2fe2eee4";
            header.RequestType = "HunterOnly";
            
            
            //************Payload***Telephone Number Request************//

            Telephone telephone = new Telephone();
            List<Telephone> telephoneList = new List<Telephone>();
            telephone.Number = (string)dt.Rows[i]["MobileNo"];
            telephone.Type = "Mobile";
            telephone.Id = "APPLICANT_1_TEL_1";
            telephoneList.Add(telephone);

            //****IdentityDocument Request******//
            IdentityDocument identityDocument = new IdentityDocument();
            identityDocument.DocumentNumber = (string)dt.Rows[i]["PanNo"];
            identityDocument.DocumentType = "PAN CARD";
            identityDocument.Id = "id1";
            List<IdentityDocument> identityDocumentList = new List<IdentityDocument>();
            identityDocumentList.Add(identityDocument);

           


             //*********Application  start*********//
            
            //****ProductDetails Request******//
            ProductDetails productDetails = new ProductDetails();
            productDetails.ProductCode = "FL_I_ES";
            productDetails.ProductType = "LOAN";


            //****Applicant*****//
            Applicant applicant = new Applicant();
            applicant.ApplicantType = "MAIN_APPLICANT";
            applicant.Id = "a028p000000V436AAC";
            applicant.ContactId = "0038p000001YEG1AAO";

            List<Applicant> applicantsLit= new List<Applicant>();
            applicantsLit.Add(applicant);

           
            Application application = new Application();
            application.ProductDetails = productDetails;
            application.Applicants = applicantsLit;
            application.OriginalRequestTime = DateTime.Now;
            application.NotificationRequired = true;

            ///////////Application End*******//


            //****Contact Request Start******//

            ///**********Address*********//
            Address address = new Address();
            address.AddressType = "PRIMARY";
            address.BuildingName = "faridabad";
            address.Id = "ADDRESS_1";
            address.Postal = "121004";
            address.PostTown = "Agra";
            address.County = "India";
            address.StateProvinceCode = "UP";
            address.Street = "h.no-105 rajiv colony ballabgrah";
            address.Street2 = "h.no-105 rajiv colony ballabgrah";
            TimeAtAddress timeAtAddress = new TimeAtAddress();
            timeAtAddress.Value = "360";
            timeAtAddress.Unit = "YEAR";
            
            address.TimeAtAddress = timeAtAddress;

            List<Address> addressList = new List<Address>();
            addressList.Add(address);
            //***********Email*********//
            Email email = new Email();
            email.EmailAddress = "vinodsha56@gmail.com";
            email.Id = "APPLICANT_1_EMAIL_1";
            email.Type = "HOME";
            List<Email> emailsList= new List<Email>(); 
            emailsList.Add(email);

            ///////////***********employmentHistory*********//
            ///
            EmploymentHistory employmentHistory = new EmploymentHistory();
            employmentHistory.Id = "MAINEMPLOYER";
            employmentHistory.EmployerName = "xyz";

            EmployerAddress employerAddress = new EmployerAddress();
            employerAddress.AddressType = "CURRENT";
            employerAddress.Street = "-";

            TimeWithEmployer timeWithEmployer = new TimeWithEmployer();
            timeWithEmployer.Unit = "MONTHS";
            timeWithEmployer.Duration = 108;

            employmentHistory.EmployerAddress = employerAddress;
            employmentHistory.TimeWithEmployer =timeWithEmployer;

            List<EmploymentHistory > employmentHistoryList = new List<EmploymentHistory>();
            employmentHistoryList.Add(employmentHistory);

            ///////////Employee Hinstory end********//
            //********Person*******//
            Person person = new Person();
            person.Id = "Person_1";
            Name name = new Name();
            name.FirstName = "";
            name.SurName = "";
            name.Id = "APPLICANT_NAME_1";
            name.Type = "CURRENT";

            List<Name> names = new List<Name>();
            names.Add(name);

            PersonDetails personDetails = new PersonDetails();
            personDetails.MaritalStatus = "MARRIED";
            personDetails.QualificationType = "DEGREE";
            personDetails.Gender = "MALE";
            personDetails.Age = 30;
            person.Names= names;
            person.PersonDetails = personDetails;

                        
            List<Contacts> contactsList = new List<Contacts>();

            Contacts contacts = new Contacts();
            contacts.Telephones = telephoneList;
            contacts.EmploymentHistory = employmentHistoryList;
            contacts.IdentityDocuments = identityDocumentList;
            contacts.Person = person;
            contacts.Emails = emailsList;
            contacts.Addresses = addressList;
            contacts.Id = "0038p000001YEG1AAO";



            contactsList.Add(contacts);


            Payload payload = new Payload();
            payload.Contacts = contactsList;
            payload.Application = application;

            requestModel.Payload = payload;
            requestModel.Header = header;
            


            return requestModel;
        }
    }
}
