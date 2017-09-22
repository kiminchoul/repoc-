using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Extensions;
//★
using System.IO;
using Microsoft.VisualBasic.FileIO;
using System.Net;
using System.Data.OleDb;
using System.Data;
//using ServiceStack;
//using ServiceStack.Text;
//★

namespace SampleRESTClient.src.main.CsharpDotNet2.IO.Swagger.Managers
{
    /**
     * This is the main class that contains the sample API calls and their outputs.
     * The sample is driven via the main method with most of API calls separated into their own
     * methods for better readability.
     */
    class ApplicationManager
    {
        // This class contains the backend functionality for sending the API request and receiving the response
        ApiClient zApi;

        // This class contains the API calls relating to the Account object
        AccountsApi accountsApi;

        // This class contains the API calls relating to the Subscription object
        SubscriptionsApi subscriptionsApi;

        //★This class contains the API calls relating to the PaymentMethod object
        PaymentMethodsApi paymentMethodsApi;
        //★This class contains the API calls relating to Action> create 
        ActionsApi actionCreateApi;

        // This class contains the API calls relating to the Catalog object
        CatalogApi catalogApi;

        //const string apiURL             = "https://rest.apisandbox.zuora.com/v1";     //zuoraIni.csvより取得する   
        //sandBox1
        //const string userID = "kimura-tomohiro@mki.co.jp.sd";
        //const string passWD = "Mki12345";
        //sandBox2
        //const string userID             = "kmj1@konicaminolta.com.sd2";
        //const string passWD             = "Kmibs123";
        const string userID             = ""; //zuoraIni.csvより取得する
        const string passWD             = ""; //zuoraIni.csvより取得する
        const string accountLog         = "file\\1resultAccount.txt";
        const string accountCSV         = "file\\1accountInfo.csv";
        const string subscMakeLog       = "file\\2resultSubscript.txt";
        const string subscHeaderCSV     = "file\\2subscriptHeader.csv";
        const string subscMakeCSV       = "file\\2subscriptInfo.csv";
        const string subscUpdateLog     = "file\\3resultCsvForUpdate.txt";
        const string subscUpdateCSV     = "file\\3csvForUpdateEdited.csv";
        const string subscListLog       = "file\\4resultMakeSubscriptCsv.txt";        
        const string subscListCSV       = "file\\4makeSubscriptCsv.csv";//更新用としてデータ修正後、3csvForUpdateEdited.csvに改名する
        const string makeSubscCsvList   = "file\\4csvForUpdate.csv";
        const string delSub             = "file\\5delSub.csv";
        const string oyaAndDate         = "file\\6oyaAndDate.txt";
        const string connectIni         = "zuoraIni.csv";
        const string csvForUpdateDB 　　= "file\\4csvForUpdateDB.csv";

        private string user;
        private string password;        

        /**
         * Constructor to take in API credentials and create the backend API client
         * Also initializes the specific API clients for each object that will be used
         * 
         * user: The Zuora API username
         * pass: The Zuora API password
         */
        public ApplicationManager(string user, string pass)
        {
            //TLS1.2対応
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            //get the connect Info
            string csvfile = connectIni; 
            TextFieldParser parser = new TextFieldParser(csvfile, Encoding.GetEncoding("UTF-8"));
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(","); // 区切り文字はコンマ
            string[] csvColumn = parser.ReadFields(); // 1行読み込み
            user = csvColumn[0];
            pass = csvColumn[1];
            string apiURL = csvColumn[2];

            //Creating the API client with the new REST endpoint
            //zApi = new ApiClient(apiURL);
            this.user = user;
            this.password = pass;

            //Adding the username and password to the header for subsequent API calls
            //zApi.AddDefaultHeader("apiAccessKeyId", user);
            //zApi.AddDefaultHeader("apiSecretAccessKey", pass);

            //Initializing API clients for Product Catalog, Accounts, and Subscriptions
            catalogApi = new CatalogApi(apiURL);
            catalogApi.AddDefaultHeader("apiAccessKeyId", user);
            catalogApi.AddDefaultHeader("apiSecretAccessKey", pass);

            accountsApi = new AccountsApi(apiURL);
            accountsApi.AddDefaultHeader("apiAccessKeyId", user);
            accountsApi.AddDefaultHeader("apiSecretAccessKey", pass);

            subscriptionsApi = new SubscriptionsApi(apiURL);
            subscriptionsApi.AddDefaultHeader("apiAccessKeyId", user);
            subscriptionsApi.AddDefaultHeader("apiSecretAccessKey", pass);

            //★ paymentMethodsApi
            paymentMethodsApi = new PaymentMethodsApi(apiURL);
            paymentMethodsApi.AddDefaultHeader("apiAccessKeyId", user);
            paymentMethodsApi.AddDefaultHeader("apiSecretAccessKey", pass);

            //★ Action>Create
            actionCreateApi = new ActionsApi(apiURL);
            actionCreateApi.AddDefaultHeader("apiAccessKeyId", user);
            actionCreateApi.AddDefaultHeader("apiSecretAccessKey", pass);
        }

        //20170720 status-Pendingのsubscriptionを作成するため 「Action/subscribe」を使用
        //★★ READ csv file to create Subscription by Action/subscribe   
        //public ProxyActionsubscribeResponse actionSubscrWithCsv(string prodId, string[] readedCsvCol, string[] readedCsvColHeader)
        //20170901 ★ratePlanDB★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
        //public string actionSubscrWithCsv(string prodId, string[] readedCsvCol, string[] readedCsvColHeader)        
        public string actionSubscrWithCsv(string prodId, string[] readedCsvCol, string[] readedCsvColHeader,DataSet datasetRP, DataSet datasetRPC, DataSet datasetAccount)
        {
            try
            {
                ProxyActionsubscribeRequest subscribeRequest = new ProxyActionsubscribeRequest();

                List<SubscribeRequest> SubscribesList = new List<SubscribeRequest>();
                SubscribeRequest Subscribes = new SubscribeRequest();

                //初期化               
                Account Account = new Account();
                Contact BillToContact = new Contact();
                PaymentMethod PaymentMethod = new PaymentMethod();
                PreviewOptions PreviewOptions = new PreviewOptions();
                Contact SoldToContact = new Contact();
                SubscribeOptions SubscribeOptions = new SubscribeOptions();
                //Subscribes.Account = Account;
                Subscribes.BillToContact = BillToContact;
                Subscribes.PaymentMethod = PaymentMethod;
                Subscribes.PreviewOptions = PreviewOptions;
                Subscribes.SoldToContact = SoldToContact;

                //SubscribeOptions.GenerateInvoice = true; //お試し：BILL RUN実行::定額のpendingのBillRunにてエラー
                Subscribes.SubscribeOptions = SubscribeOptions;

                // 2-SubscriptionData
                SubscriptionData SubscriptionData = new SubscriptionData();
                // 2-1 RatePlanData
                List<RatePlanData> RatePlanDataList = new List<RatePlanData>();
                RatePlanData RatePlanData = new RatePlanData();

                // ▼▼▼ratePlan
                //2-1-1 RatePlan
                RatePlan RatePlan = new RatePlan();
                RatePlan.ProductRatePlanId = prodId;

                /** //20170901 ★ratePlanDB★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
                // 品目コードをratePlan objectに登録                
                //ProxyActionqueryRequest zPrtRatePlan = new ProxyActionqueryRequest();
                //zPrtRatePlan.QueryString = "select ProductCode__c from ProductRatePlan where id = '" + prodId + "'";
                //string selHinmok = actionCreateApi.ProxyActionPOSTquery(zPrtRatePlan);
                //品目コードがNULLの場合、ProductRatePlanIDがRETURNされるので（親品目）
                // ⇒ 親も品目コードセットされ、親品目コードがNULLとなることに。。。ロジックはこのまま生かす。品目コード未設定のエラー対応として
                //if (selHinmok == prodId)
                //    RatePlan.ProductCode__c = null;
                //else
                //    RatePlan.ProductCode__c = selHinmok;
                */                                
                string selHinmok = null;
                foreach (DataTable table in datasetRP.Tables)
                {
                    DataRow[] dataRows = table.Select("[0_rateplanid] = '" + prodId + "'");
                    selHinmok = (string)dataRows[0][2];                                                            
                }
                if (!string.IsNullOrEmpty(selHinmok))
                    RatePlan.ProductCode__c = selHinmok;
                //20170901 ★ratePlanDB★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★

                // 親品目コードをratePlan objectに登録
                // 親 ->「取得した品目コード(selHinmok)＝CSVの親品目コード(readedCsvCol[3])」) -> 親品目コード＝NULL　
                if (selHinmok.Equals(readedCsvCol[3]))
                    RatePlan.ParentCode__c = null;
                else
                    RatePlan.ParentCode__c = readedCsvCol[3];
                // NeTTS製番NeTTSProduct__c=0000、NeTTS号機NeTTSUnit__c=00000
                RatePlan.NeTTSProduct__c = "0000";
                RatePlan.NeTTSUnit__c = "00000";

                /** //20170901 ★ratePlanDB★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
                // 課金対象フラグ(BillingFlag__c)で、TRUE→親はS販売単価を子はマスタの値を、FALSE→ゼロを
                zPrtRatePlan.QueryString = "select BillingFlag__c from ProductRatePlan where id = '" + prodId + "'";
                string seledBillFlg = actionCreateApi.ProxyActionPOSTquery(zPrtRatePlan);
                if (!seledBillFlg.Equals(prodId))  //フラグ未設定の場合、prodIdが返ってくるので
                    RatePlan.BillingFlag__c = seledBillFlg;
                */
                string seledBillFlg = null;
                foreach (DataTable table in datasetRP.Tables)
                {
                    DataRow[] dataRows = table.Select("[0_rateplanid] = '" + prodId + "'");
                    seledBillFlg = (string)dataRows[0][5];
                }
                if (!string.IsNullOrEmpty(seledBillFlg))
                    RatePlan.BillingFlag__c = seledBillFlg;
                //20170901 ★ratePlanDB★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★

                //add st KMIBS-107 ratePlanにカスタム項目の利用許諾要否フラグ追加
                /** //20170901 ★ratePlanDB★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
                // 利用許諾要否フラグ（RequiredLicense__c）
                zPrtRatePlan.QueryString = "select RequiredLicense__c from ProductRatePlan where id = '" + prodId + "'";
                string seledRequired = actionCreateApi.ProxyActionPOSTquery(zPrtRatePlan);
                if (!seledRequired.Equals(prodId))  //フラグ未設定の場合、prodIdが返ってくるので
                    RatePlan.RequiredLicense__c = seledRequired;
                */
                string seledRequired = null;
                foreach (DataTable table in datasetRP.Tables)
                {
                    DataRow[] dataRows = table.Select("[0_rateplanid] = '" + prodId + "'");
                    seledRequired = (string)dataRows[0][6];
                }
                if (!string.IsNullOrEmpty(seledRequired))
                    RatePlan.RequiredLicense__c = seledRequired;
                //20170901 ★ratePlanDB★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
                //add ed KMIBS-107

                RatePlanData.RatePlan = RatePlan;  //2-1-1
                //▲▲▲ RatePlan

                //2-1-2 RatePlanChargeData
                List<RatePlanChargeData> RatePlanChargeDataList = new List<RatePlanChargeData>();
                RatePlanChargeData RatePlanChargeData = new RatePlanChargeData();

                // ▼▼▼ratePlanCharge
                //2-1-2-1 RatePlanCharge
                //ProductRatePlanChargeID

                /** //20170901 ★ratePlanChargeDB★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
                ProxyActionqueryRequest zPrtRatePlanCharge = new ProxyActionqueryRequest();
                zPrtRatePlanCharge.QueryString = "select id from ProductRatePlanCharge where ProductRatePlanId = '" + prodId + "'";
                RatePlanCharge RatePlanCharge = new RatePlanCharge();
                RatePlanCharge.ProductRatePlanChargeId = actionCreateApi.ProxyActionPOSTquery(zPrtRatePlanCharge);
                */
                RatePlanCharge RatePlanCharge = new RatePlanCharge();
                foreach (DataTable table in datasetRPC.Tables)
                {
                    DataRow[] dataRows = table.Select("[4_rateplanid] = '" + prodId + "'");
                    RatePlanCharge.ProductRatePlanChargeId = (string)dataRows[0][0];
                }
                //20170901 ★ratePlanDB★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★

                //課金対象フラグ(BillingFlag__c)で、TRUE→親はS販売単価を子はマスタの値を、FALSE→ゼロを                
                //if (seledBillFlg.Equals("true") || seledBillFlg.Equals("TRUE"))                
                if (bool.Parse(seledBillFlg).Equals(true))
                {
                    if (selHinmok.Equals(readedCsvCol[3]))//課金対象フラグBillingFlag__c=true、親の場合はS販売単価をセット                
                        if (!string.IsNullOrEmpty(readedCsvCol[8]))
                            RatePlanCharge.Price = double.Parse(readedCsvCol[8]);
                    //子はマスタの値:何もしない(マスタ値）
                }
                else
                    RatePlanCharge.Price = double.Parse("0");

                // ST_DEL KMIBS-25 
                // 明細CSVの数量が２以上はDBAなので子品目の数量に設定する。
                //// CSV Detail 5_数量 : 親 + Per Unit Pricing
                //ProxyActionqueryRequest zRatePlanChargeQuantity = new ProxyActionqueryRequest();
                //zRatePlanChargeQuantity.QueryString = "select ChargeModel from ProductRatePlanCharge where ProductRatePlanId = '" + prodId + "'";
                //string chargeModel = actionCreateApi.ProxyActionPOSTquery(zRatePlanChargeQuantity);
                //// Per Unit PricingのみDESUNE with only type" : "Recurring    a            
                //if (selHinmok.Equals(readedCsvCol[3]))
                //    if (chargeModel.Equals("Per Unit Pricing"))
                //        if (!string.IsNullOrEmpty(readedCsvCol[5]))
                //            RatePlanCharge.Quantity = double.Parse(readedCsvCol[5]);
                // ED_DEL KMIBS-25 

                // ST_MOD KMIBS-64
                // ST_ADD KMIBS-25 
                //{"Errors":{"Code":"MISSING_REQUIRED_VALUE","Message":
                //"A quantity is required for One-time and Recurring charge types with Per Unit, Tiered, and Volume charge models."},"Success":false}
                /** //20170901 ★ratePlanChargeDB★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
                ProxyActionqueryRequest zRatePlanChargeQuantity = new ProxyActionqueryRequest();
                //chargeModel
                zRatePlanChargeQuantity.QueryString = "select ChargeModel from ProductRatePlanCharge where ProductRatePlanId = '" + prodId + "'";
                string chargeModel = actionCreateApi.ProxyActionPOSTquery(zRatePlanChargeQuantity);
                */
                string chargeModel = null;
                foreach (DataTable table in datasetRPC.Tables)
                {
                    DataRow[] dataRows = table.Select("[4_rateplanid] = '" + prodId + "'");
                    chargeModel = (string)dataRows[0][2];
                }                
                //20170901 ★ratePlanDB★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★

                //chargeType
                /** //20170901 ★ratePlanChargeDB★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
                zRatePlanChargeQuantity.QueryString = "select ChargeType from ProductRatePlanCharge where ProductRatePlanId = '" + prodId + "'";
                string chargeType = actionCreateApi.ProxyActionPOSTquery(zRatePlanChargeQuantity);
                */
                string chargeType = null;
                foreach (DataTable table in datasetRPC.Tables)
                {
                    DataRow[] dataRows = table.Select("[4_rateplanid] = '" + prodId + "'");
                    chargeType = (string)dataRows[0][3];
                }
                //20170901 ★ratePlanDB★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★

                // Per Unit PricingのみDESUNE with only type : Recurring
                //if (selHinmok.Equals(readedCsvCol[3]))
                //    if (chargeModel.Equals("Per Unit Pricing"))   // DBA親品目も「単位あたり」に変更された。
                //        RatePlanCharge.Quantity = int.Parse("1");                                                
                //// 明細CSVの数量が２以上はDBAなので子品目の数量に設定する。
                //if (!selHinmok.Equals(readedCsvCol[3])) //子品目：品目コード＜＞親品目コード
                //    if (int.Parse(readedCsvCol[5]) > 1)
                //        RatePlanCharge.Quantity = int.Parse(readedCsvCol[5]);
                // ED_ADD KMIBS-25 
                /// 数量２以上がDBAではなかった。数量１のDBAもあってリハ２にてエラーになりました。              
                /// 単位当たり料金の場合、数量をセットしないといけないので明細の数量をセットする。
                if (chargeModel.Equals("Per Unit Pricing") && chargeType.Equals("Recurring"))
                    RatePlanCharge.Quantity = int.Parse(readedCsvCol[5]);                
                // ED_MOD KMIBS-64

                // ST_ADD KMIBS-25 
                // S販単価：０
                RatePlanCharge.UserPrice__c = "0";
                // ED_ADD KMIBS-25 

                RatePlanChargeData.RatePlanCharge = RatePlanCharge; //2-1-2-1
                //▲▲▲ RatePlanCharge

                RatePlanChargeDataList.Add(RatePlanChargeData);
                RatePlanData.RatePlanChargeData = RatePlanChargeDataList; //2-1-2

                RatePlanDataList.Add(RatePlanData);
                SubscriptionData.RatePlanData = RatePlanDataList; // 2-1 RatePlanData

                // 2-2 Subscription
                Subscription Subscription = new Subscription();
                /** // CSV Header 4、5_納入先、請求先
                    // Harisの受注CSVのアカウントID,請求先IDに紐づくaccountIdをサブスクリプションの顧客名、請求先名にセットする。
                */
                if (readedCsvColHeader[4].Equals(readedCsvColHeader[5]))
                {   //契約先＝請求先：１回検索

                   /** //20170901 ★ratePlanDB★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
                    ProxyActionqueryRequest zAccount = new ProxyActionqueryRequest();
                    zAccount.QueryString = "select id from account where HARISCostomerNumber__c = '" + readedCsvColHeader[4] + "'";
                    Subscription.AccountId = actionCreateApi.ProxyActionPOSTquery(zAccount);
                    Subscription.InvoiceOwnerId = Subscription.AccountId;
                    */                    
                    foreach (DataTable table in datasetAccount.Tables)
                    {
                        DataRow[] dataRows = table.Select("[2_HARISCostomerNumber__c] = '" + readedCsvColHeader[4] + "'");
                        Subscription.AccountId = (string)dataRows[0][0];
                        Subscription.InvoiceOwnerId = Subscription.AccountId;
                    }
                    //20170901 ★ratePlanDB★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
                }
                else
                {   //契約先 <> 請求先：それぞれ検索
                    /** //20170901 ★ratePlanDB★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
                    ProxyActionqueryRequest zAccount = new ProxyActionqueryRequest();
                    zAccount.QueryString = "select id from account where HARISCostomerNumber__c = '" + readedCsvColHeader[4] + "'";
                    Subscription.AccountId = actionCreateApi.ProxyActionPOSTquery(zAccount);
                    */
                    foreach (DataTable table in datasetAccount.Tables)
                    {
                        DataRow[] dataRows = table.Select("[2_HARISCostomerNumber__c] = '" + readedCsvColHeader[4] + "'");
                        Subscription.AccountId = (string)dataRows[0][0];                        
                    }
                    //20170901 ★ratePlanDB★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★

                    /** //20170901 ★ratePlanDB★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
                    zAccount.QueryString = "select id from account where HARISCostomerNumber__c = '" + readedCsvColHeader[5] + "'";
                    Subscription.InvoiceOwnerId = actionCreateApi.ProxyActionPOSTquery(zAccount);
                    */
                    foreach (DataTable table in datasetAccount.Tables)
                    {
                        DataRow[] dataRows = table.Select("[2_HARISCostomerNumber__c] = '" + readedCsvColHeader[5] + "'");
                        Subscription.InvoiceOwnerId = (string)dataRows[0][0];
                    }
                    //20170901 ★ratePlanDB★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
                }

                //　CSV Header 1_試算番号
                if (!string.IsNullOrEmpty(readedCsvColHeader[1]))
                    Subscription.QuoteNumberQT = readedCsvColHeader[1];

                // CSV Header 3_自動更新区分
                //if (!string.IsNullOrEmpty(readedCsvColHeader[3]))
                //{
                //if (readedCsvColHeader[3].Equals("true"))
                if (readedCsvColHeader[3].Equals("X"))
                    Subscription.AutoRenew = true;
                else Subscription.AutoRenew = false;
                //}

                //Accountオブジェクトは必須 ：AccountNumberだと新規のAccountの作成となる
                Account.Id = Subscription.AccountId;
                Subscribes.Account = Account;

                // CSV Detail 9_契約開始日
                if (!string.IsNullOrEmpty(readedCsvCol[9]))
                {
                    /// mod st KMIBS-107  １有効化待ちサブスクリプションを有効化                                       
                    ///zPrtRatePlan.QueryString = "select RequiredLicense__c from ProductRatePlan where id = '" + prodId + "'";
                    ///string seledRequired = actionCreateApi.ProxyActionPOSTquery(zPrtRatePlan);
                    ///DateTime dateContract = DateTime.Parse(readedCsvCol[9]);                    
                    /// //RequiredLicense__c = trueの場合、status＝Pending ActivationのためContractEffectiveDateのみ設定
                    ///if (seledRequired.Equals("true"))
                    ///{
                    ///    Subscription.ContractEffectiveDate = dateContract;
                    ///}
                    ///else
                    ///{
                    ///    Subscription.ContractEffectiveDate = dateContract;
                    ///    Subscription.ServiceActivationDate = dateContract;
                    ///    Subscription.ContractAcceptanceDate = dateContract;
                    ///}
                    DateTime dateContract = DateTime.Parse(readedCsvCol[9]);
                    Subscription.ContractEffectiveDate = dateContract;
                    Subscription.ServiceActivationDate = dateContract;
                    Subscription.ContractAcceptanceDate = dateContract;
                    //mod ed KMIBS-107
                }
                ///★test EndDate  //field not insertable
                ///if (!string.IsNullOrEmpty(readedCsvCol[10])){
                ///    Subscription.SubscriptionEndDate = DateTime.Parse(readedCsvCol[10]);
                ///    Subscription.TermEndDate = DateTime.Parse(readedCsvCol[10]);
                ///}
                //mod st KMIBS-107  カスタム項目追加
                //19_ERP見積番号
                if (!string.IsNullOrEmpty(readedCsvCol[19]))
                    Subscription.ERPQuoteNumber__c = readedCsvCol[19];
                //20_見積明細番号
                if (!string.IsNullOrEmpty(readedCsvCol[20]))
                    Subscription.ERPQuoteDetailsNumber__c = readedCsvCol[20];
                //mod ed KMIBS-107                

                // KMIBS - 21 期間開始日を明細CSVに追加（readedCsvCol[19]）して、TermStartDateに登録する
                if (!string.IsNullOrEmpty(readedCsvCol[21]))
                    Subscription.TermStartDate = DateTime.Parse(readedCsvCol[21]);

                // ST_ADD KMIBS-48 サービス開始日　//なぜかDateTimeではエラーになるので、文字列の"YYYY-MM-DD"とする
                if (!string.IsNullOrEmpty(readedCsvCol[9]))
                {
                    //yyyy/m/dの対応
                    //Subscription.ServiceStartDate__c = readedCsvCol[9];
                    DateTime dateKaishi = DateTime.Parse(readedCsvCol[9]);
                    //string kaisiDate = readedCsvCol[9].Replace("/", "-");
                    string kaisiDate = dateKaishi.ToString("yyyy-MM-dd");
                    Subscription.ServiceStartDate__c = kaisiDate;
                }
                // ED_ADD KMIBS-48

                //Subscription.Notes = "20170731";
                //必須項目                
                //CSV Detail 12_契約期間数、13_更新期間数                
                if (!string.IsNullOrEmpty(readedCsvCol[12]))
                {
                    Subscription.TermType = "TERMED";
                    // CSV Detail 12_契約期間数
                    Subscription.InitialTerm = int.Parse(readedCsvCol[12]);
                    // CSV Detail 13_更新期間数
                    if (!string.IsNullOrEmpty(readedCsvCol[13]))
                        Subscription.RenewalTerm = int.Parse(readedCsvCol[13]);
                    else
                        Subscription.RenewalTerm = int.Parse(readedCsvCol[12]); //NULLの場合、初期契約期間
                }
                else
                    Subscription.TermType = "EVERGREEN";

                // CSV Detail 14_HARIS伝票区分、15_受注伝票番号、16_受注明細番号
                if (!string.IsNullOrEmpty(readedCsvCol[14]))
                    Subscription.ProcessStatus__c = readedCsvCol[14];
                if (!string.IsNullOrEmpty(readedCsvCol[15]))
                    Subscription.OrderNumber__c = readedCsvCol[15];
                if (!string.IsNullOrEmpty(readedCsvCol[16]))
                    Subscription.OrderDetailsNumber__c = readedCsvCol[16];
                
                SubscriptionData.Subscription = Subscription; // 2-2 Subscription
                Subscribes.SubscriptionData = SubscriptionData; //2-SubscriptionData
                SubscribesList.Add(Subscribes);
                subscribeRequest.Subscribes = SubscribesList;

                // カスタム項目：プラン数量の取得
                // プラン数量(PlanQuantity__c)分のサブスクリプションを作成する
                /** //20170901 ★ratePlanDB★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
                zPrtRatePlan.QueryString = "select PlanQuantity__c from ProductRatePlan where id = '" + prodId + "'";                                
                string strCount = actionCreateApi.ProxyActionPOSTquery(zPrtRatePlan);
                */
                string strCount = null;
                int countInt = 0;
                foreach (DataTable table in datasetRP.Tables)
                {
                    DataRow[] dataRows = table.Select("[0_rateplanid] = '" + prodId + "'");
                    countInt = (int)dataRows[0][4];
                    strCount = countInt.ToString();
                }
                //20170901 ★ratePlanDB★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★

                string localVarResponse = null;
                                
                //契約先がNULL
                if (string.IsNullOrEmpty(readedCsvColHeader[4]) || string.IsNullOrEmpty(Subscription.AccountId))
                {
                    Console.Out.WriteLine("契約先を確認してください。 " );
                }
                //請求先がNULL
                else if (string.IsNullOrEmpty(readedCsvColHeader[5]) || string.IsNullOrEmpty(Subscription.InvoiceOwnerId))
                {
                    Console.Out.WriteLine("請求先を確認してください。 "　);
                }
                else
                {
                    //NULL対応   prodIdがRETURNされる。。。
                    //if (!string.IsNullOrEmpty(strCount)) 
                    if (strCount == prodId)
                    {
                        localVarResponse = actionCreateApi.ProxyActionPOSTsubscribe(subscribeRequest);
                    }
                    else
                    {
                        long planCount = long.Parse(strCount);
                        if (planCount > 1)
                        {
                            //（strCount-1）件分を作成し、最後は全体ロジックにて処理するして呼出元のプロシージャに戻る
                            for (int i = 1; i < planCount; i++)
                            {
                                localVarResponse = actionCreateApi.ProxyActionPOSTsubscribe(subscribeRequest);
                                Console.Out.WriteLine(planCount + "ProxyActionPOSTsubscribe（PlanQuantity__c）: " + localVarResponse.ToString());
                            }
                        }
                        //ゼロ対応：ゼロ入力はいたずら？                    
                        if (planCount > 0)
                            localVarResponse = actionCreateApi.ProxyActionPOSTsubscribe(subscribeRequest);
                    }
                }

                return localVarResponse;                
            }
            catch (Exception ex)
            {
                Console.WriteLine("actionSubscrWithCsv() failed with message: " + ex.Message);
                // 20170721 Cannot deserialize the current JSON array～対応
                //ProxyActionsubscribeResponse localVarResponse = null;
                string localVarResponse = null;
                return localVarResponse;
            }
        }
                
        //★★ READ csv file to create Subscription
        //async
        //public async System.Threading.Tasks.Task<POSTSubscriptionResponseType> createSubscrWithCsv(string prodId, string[] readedCsvCol)
        //sync
        //public POSTSubscriptionResponseType createSubscrWithCsv(string prodId, string[] readedCsvCol)
        public POSTSubscriptionResponseType createSubscrWithCsv(string prodId, string[] readedCsvCol, string[] readedCsvColHeader)
        {
            try
            {
                //Initialize the Subscription container
                POSTSubscriptionType subscription = new POSTSubscriptionType();

                //▼ RatePlan
                //Initialize the Rate Plan container list (Must use a list as the subscription can have multiple rate plans)
                List<POSTSrpCreateType> ratePlanList = new List<POSTSrpCreateType>();
                //Initialize the individual Rate Plan container
                POSTSrpCreateType ratePlan = new POSTSrpCreateType();
                //Populate the Rate Plan container with the required field
                ratePlan.ProductRatePlanId = prodId;

                //★■ 品目コードをratePlan objectに登録
                // ToDo:prodIdでProductRatePlanを検索して取得する。
                ProxyActionqueryRequest zPrtRatePlan = new ProxyActionqueryRequest();
                zPrtRatePlan.QueryString = "select ProductCode__c from ProductRatePlan where id = '" + prodId + "'";
                string selHinmok = actionCreateApi.ProxyActionPOSTquery(zPrtRatePlan);
                //品目コードがNULLの場合、ProductRatePlanIDがRETURNされるので（親品目）
                // ⇒ 親も品目コードセットされ、親品目コードがNULLとなることに。。。ロジックはこのまま生かす。品目コード未設定のエラー対応として
                if (selHinmok == prodId)
                    ratePlan.ProductCode__c = null;
                else
                    ratePlan.ProductCode__c = selHinmok;

                //★■ 親品目コードをratePlan objectに登録
                // 親 ->「取得した品目コード(selHinmok)　＝　CSVの親品目コード(readedCsvCol[3])」) -> 親品目コード＝NULL　
                if (selHinmok.Equals(readedCsvCol[3]))
                    ratePlan.ParentCode__c = null;
                else
                    ratePlan.ParentCode__c = readedCsvCol[3];

                //■★ 20170718 NeTTS製番NeTTSProduct__c=0000、NeTTS号機NeTTSUnit__c=00000
                ratePlan.NeTTSProduct__c = "0000";
                ratePlan.NeTTSUnit__c = "00000";

                // ▼▼▼ratePlanChargeセット
                List<POSTScCreateType> ratePlanChargeList = new List<POSTScCreateType>();
                POSTScCreateType ratePlanCharge = new POSTScCreateType();
                /** //★■  代理店向け価額 （8_S販売単価）
                //親品目の場合（品目コードがNULL）、代理店向け価額を表示する ⇒ 親品目コードがNULLになりました。。。
                //if (string.IsNullOrEmpty(ratePlan.ProductCode__c))
                if (string.IsNullOrEmpty(ratePlan.ParentCode__c))
                {
                    if (!string.IsNullOrEmpty(readedCsvCol[8]))
                    {
                        ratePlanCharge.UserPrice__c = readedCsvCol[8];
                    }                    
                }
                */

                //課金対象フラグBillingFlag__cの取得
                //■★ 20170718 課金対象フラグ(BillingFlag__c)で、TRUE→親はS販売単価を子はマスタの値を、FALSE→ゼロを
                zPrtRatePlan.QueryString = "select BillingFlag__c from ProductRatePlan where id = '" + prodId + "'";
                string seledBillFlg = actionCreateApi.ProxyActionPOSTquery(zPrtRatePlan);
                if (seledBillFlg.Equals("true"))
                {
                    if (selHinmok.Equals(readedCsvCol[3]))//課金対象フラグBillingFlag__c=true、親の場合はS販売単価をセット                
                        ratePlanCharge.Price = readedCsvCol[8];
                    //子はマスタの値:何もしない
                }
                else
                    ratePlanCharge.Price = "0";

                //■★ 20170718 CSV Detail 5_数量 : 親 + Per Unit Pricing
                ProxyActionqueryRequest zRatePlanChargeQuantity = new ProxyActionqueryRequest();
                zRatePlanChargeQuantity.QueryString = "select ChargeModel from ProductRatePlanCharge where ProductRatePlanId = '" + prodId + "'";
                string chargeModel = actionCreateApi.ProxyActionPOSTquery(zRatePlanChargeQuantity);
                // Per Unit PricingのみDESUNE with only type" : "Recurring                
                if (selHinmok.Equals(readedCsvCol[3]))
                    if (chargeModel.Equals("Per Unit Pricing"))
                        if (!string.IsNullOrEmpty(readedCsvCol[5]))
                            ratePlanCharge.Quantity = readedCsvCol[5];

                //ProductRatePlanChargeID
                ProxyActionqueryRequest zPrtRatePlanCharge = new ProxyActionqueryRequest();
                zPrtRatePlanCharge.QueryString = "select id from ProductRatePlanCharge where ProductRatePlanId = '" + prodId + "'";
                ratePlanCharge.ProductRatePlanChargeId = actionCreateApi.ProxyActionPOSTquery(zPrtRatePlanCharge);
                ratePlanChargeList.Add(ratePlanCharge);
                //▲▲▲ RatePlanCharge

                //■★ 20170719
                ratePlan.BillingFlag__c = seledBillFlg;

                ratePlan.ChargeOverrides = ratePlanChargeList;
                //Add the individual Rate Plan container to the list
                ratePlanList.Add(ratePlan);
                //▲ RatePlan

                //Add the list of Rate Plans to the Subscription container
                subscription.SubscribeToRatePlans = ratePlanList;

                //■★ 20170718 CSV Header 1_試算番号
                if (!string.IsNullOrEmpty(readedCsvColHeader[1]))
                    subscription.QuoteNumberQT = readedCsvColHeader[1];

                //■★ 20170718 CSV Header 3_自動更新区分
                if (!string.IsNullOrEmpty(readedCsvColHeader[3]))
                {
                    if (readedCsvColHeader[3].Equals("true"))
                        subscription.AutoRenew = true;
                    else subscription.AutoRenew = false;
                }

                /** //Header CSVにてHaris請求先IDがセットされることに。。。
                if (!string.IsNullOrEmpty(readedCsvColHeader[5]))
                {                    
                    subscription.InvoiceOwnerName__c = readedCsvColHeader[5];
                }  
                */
                /** //アカウント名だったらAccountIDを取得
                if (!string.IsNullOrEmpty(readedCsvColHeader[4]))
                {
                    subscription.AccountKey = readedCsvColHeader[4];
                    //CSVのAccountKeyがアカウント名だったらAccountIdを取得する。                    
                    ProxyActionqueryRequest zAccount = new ProxyActionqueryRequest();
                    zAccount.QueryString = "select id from account where name = '" + subscription.AccountKey + "'";
                    string accountId = actionCreateApi.ProxyActionPOSTquery(zAccount);
                    if (!string.IsNullOrEmpty(accountId)) //もし、CSVのAccountKeyがaccountIDだったら、string accountId = NULLなので
                        subscription.AccountKey = accountId;                    
                }
                */
                /** // CSV Header 4、5_納入先、請求先
                Harisの受注CSVのアカウントID,請求先IDに紐づくaccountIdをサブスクリプションの顧客名、請求先名にセットする。
                */
                if (readedCsvColHeader[4].Equals(readedCsvColHeader[5]))
                {   //契約先＝請求先：１回検索
                    ProxyActionqueryRequest zAccount = new ProxyActionqueryRequest();
                    zAccount.QueryString = "select id from account where HARISCostomerNumber__c = '" + readedCsvColHeader[4] + "'";
                    subscription.AccountKey = actionCreateApi.ProxyActionPOSTquery(zAccount);
                }
                else
                {   //契約先 <> 請求先：それぞれ検索
                    ProxyActionqueryRequest zAccount = new ProxyActionqueryRequest();
                    zAccount.QueryString = "select id from account where HARISCostomerNumber__c = '" + readedCsvColHeader[4] + "'";
                    subscription.AccountKey = actionCreateApi.ProxyActionPOSTquery(zAccount);

                    zAccount.QueryString = "select id from account where HARISCostomerNumber__c = '" + readedCsvColHeader[5] + "'";
                    subscription.InvoiceOwnerAccountKey = actionCreateApi.ProxyActionPOSTquery(zAccount);
                }

                /** //status制御 利用許諾要否フラグRequiredLicense__c
                //■★ 20170718 利用許諾要否フラグ(RequiredLicense__c)で、TRUE→STATUS:pending、FALSE→STATUS:active
                // status:Pendingに出来るの？？？
                zPrtRatePlan.QueryString = "select RequiredLicense__c from ProductRatePlan where id = '" + prodId + "'";
                string seledRequired = actionCreateApi.ProxyActionPOSTquery(zPrtRatePlan);
                if (seledRequired.Equals("false"))
                subscription.Status = "Active"; //利用許諾要否フラグ=false                
                else
                subscription.Status = "Draft"; //利用許諾要否フラグ=true
                */

                //Populate the subscription with all other required fields
                //■★ 20170718 CSV Detail 9_契約開始日
                if (!string.IsNullOrEmpty(readedCsvCol[9]))
                {
                    DateTime dateContract = DateTime.Parse(readedCsvCol[9]);
                    subscription.ContractEffectiveDate = dateContract;
                }
                //■★ 20170718 CSV Detail 11_解約日                
                /**
                if (!string.IsNullOrEmpty(readedCsvCol[11]))
                {
                    DateTime dateContract = DateTime.Parse(readedCsvCol[11]);
                    subscription.CancelledDate  = dateContract;
                }
                */
                //■★ 20170718 CSV Detail 12_契約期間数、13_更新期間数
                if (!string.IsNullOrEmpty(readedCsvCol[12]))
                {
                    subscription.TermType = "TERMED";
                    // CSV Detail 12_契約期間数
                    subscription.InitialTerm = long.Parse(readedCsvCol[12]);
                    // CSV Detail 13_更新期間数
                    if (!string.IsNullOrEmpty(readedCsvCol[13]))
                        subscription.RenewalTerm = long.Parse(readedCsvCol[13]);
                }
                else
                    subscription.TermType = "EVERGREEN";

                //■★ 20170718 CSV Detail 14_HARIS伝票区分、15_受注伝票番号、16_受注明細番号
                if (!string.IsNullOrEmpty(readedCsvCol[14]))
                    subscription.ProcessStatus__c = readedCsvCol[14];
                if (!string.IsNullOrEmpty(readedCsvCol[15]))
                    subscription.OrderNumber__c = readedCsvCol[15];
                if (!string.IsNullOrEmpty(readedCsvCol[16]))
                    subscription.OrderDetailsNumber__c = readedCsvCol[16];

                //プラン数量_PlanQuantity__cの取得
                //■★ 20170719 プラン数量分のサブスクリプションを作成する
                zPrtRatePlan.QueryString = "select PlanQuantity__c from ProductRatePlan where id = '" + prodId + "'";
                string strCount = actionCreateApi.ProxyActionPOSTquery(zPrtRatePlan);
                long planCount = long.Parse(strCount);
                // 件数ー１分作成し、ラストは全体ロジックにて処理する。。。           
                if (planCount > 1)
                {
                    for (int i = 1; i < planCount; i++)
                    {
                        /** //Async
                        //return await subscriptionsApi.POSTSubscriptionAsync(subscription, "211.0").ConfigureAwait(false);
                        //sync 
                        //作成済みのSubscriptionKEY取得してSTATUS更新できるかな～
                        //return subscriptionsApi.POSTSubscriptionSync(subscription, "211.0");
                        */
                        POSTSubscriptionResponseType localVarResponseFor = null;
                        localVarResponseFor = subscriptionsApi.POSTSubscriptionSync(subscription, "211.0");
                        /** //■★ 20170718 For Update the STATUS of Subscription
                        //Initialize the container
                        ProxyModifySubscription zSubscription = new ProxyModifySubscription();
                        zSubscription.Notes  = "to do status value is Draft";
                        //zSubscription.Status = "PendingActivation";                                
                        DateTime dateConAccep = DateTime.Parse(readedCsvCol[9]);
                        zSubscription.ServiceActivationDate = dateConAccep;
                        zSubscription.ContractEffectiveDate = dateConAccep;
                        zSubscription.ContractAcceptanceDate = null;
                        ProxyCreateOrModifyResponse localVarResponseUpdate = null;
                        localVarResponseUpdate = subscriptionsApi.ProxyPUTSubscription(localVarResponse.SubscriptionId, zSubscription);
                        */
                        //return localVarResponse;
                        Console.Out.WriteLine("POSTSubscriptionResponseType: " + localVarResponseFor.ToString());
                    }
                }

                /** //Async
                    //return await subscriptionsApi.POSTSubscriptionAsync(subscription, "211.0").ConfigureAwait(false);
                    //sync 
                    //作成済みのSubscriptionKEY取得してSTATUS更新できるかな～
                    //return subscriptionsApi.POSTSubscriptionSync(subscription, "211.0");
                    */
                POSTSubscriptionResponseType localVarResponse = null;
                localVarResponse = subscriptionsApi.POSTSubscriptionSync(subscription, "211.0");
                /** //■★ 20170718 For Update the STATUS of Subscription
                    //Initialize the container
                    ProxyModifySubscription zSubscription = new ProxyModifySubscription();
                    zSubscription.Notes  = "to do status value is Draft";
                    //zSubscription.Status = "PendingActivation";                                
                    DateTime dateConAccep = DateTime.Parse(readedCsvCol[9]);
                    zSubscription.ServiceActivationDate = dateConAccep;
                    zSubscription.ContractEffectiveDate = dateConAccep;
                    zSubscription.ContractAcceptanceDate = null;
                    ProxyCreateOrModifyResponse localVarResponseUpdate = null;
                    localVarResponseUpdate = subscriptionsApi.ProxyPUTSubscription(localVarResponse.SubscriptionId, zSubscription);
                    */
                return localVarResponse;

            }
            catch (Exception ex)
            {
                Console.WriteLine("createSubscrWithCsv() failed with message: " + ex.Message);
                POSTSubscriptionResponseType localVarResponse = null;
                return localVarResponse;
            }
        }

        //
        /// KMIBS-54 親subScription番号を更新登録する
        /// 29_QuoteNumber__QT、49_ParentCode__c、52_ProductCode__c、8_subscriptionNumber
        /// 20170901 dataSetを毎回読込むのに時間がかかるので、１回のみ読んで引数で渡す
        //public PUTSubscriptionResponseType updateOyaSubscriptionNo(string[] readedCsvCol)
        public PUTSubscriptionResponseType updateOyaSubscriptionNo(string[] readedCsvCol, DataSet dataset)
        {
            try
            {                
                //Initialize the Subscription container
                PUTSubscriptionType subscription = new PUTSubscriptionType();

                /** csvファイルをDB化して検索の効率を上げる
                //ProxyActionqueryRequest zOyaNo = new ProxyActionqueryRequest();
                //zOyaNo.QueryString = "select Name from subscription where QuoteNumber__QT = '" + readedCsvCol[29] + "' and ";
                //subscription.ParentSubscriptionNumber__c = actionCreateApi.ProxyActionPOSTquery(zOyaNo);   
                
                //★ READ csvDB file : 4csvForUpdate.csvファイルをもう一度読込
                string csvfile = makeSubscCsvList;   //CSVファイル格納場所
                TextFieldParser parser = new TextFieldParser(csvfile, Encoding.GetEncoding("UTF-8"));
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(","); // 区切り文字はコンマ
                              
                int countCsv = 0;
                while (!parser.EndOfData)
                {
                    string[] csvDB = parser.ReadFields(); // 1行読み込み
                    /// 29_QuoteNumber__QT、49_ParentCode__c、52_ProductCode__c、8_subscriptionNumber、11_contractEffectiveDate
                    
                    if (readedCsvCol[29].Equals(csvDB[29]))          //csv.試算番号     = DB.試算番号
                        if (readedCsvCol[49].Equals(csvDB[52]))      //csv.親品目コード = DB.品目コード
                            if (readedCsvCol[11].Equals(csvDB[11]))  //csv.契約有効日   = DB.契約有効日 : リハ２にて明細CSVに同一試算番号、品目コードのデータがあったので
                            {
                                subscription.ParentSubscriptionNumber__c = csvDB[8];
                                break;
                            }
                    countCsv += 1;
                    continue;//次のCSV処理に 
                } 
                */

                /** 20170901 dataSetを毎回読込むのに時間がかかるので、１回のみ読んで引数で渡す
                // CSV形式のテキストファイルの内容をDataTableに直接格納            
                string filename = csvForUpdateDB;   //CSVファイル格納場所（実行ファイル同）

                // データベースへ接続する(今回はCSVファイルを開く)
                OleDbConnection connection = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0; Data Source=" + Path.GetDirectoryName(filename) + "\\; Extended Properties=\"Text;HDR=YES;FMT=Delimited\"");

                // クエリ文字列を作る
                // ※ファイルパスを[]でくくること！
                OleDbCommand command = new OleDbCommand("SELECT * FROM [" + Path.GetFileName(filename) + "]", connection);

                DataSet dataset = new DataSet();
                dataset.Clear();    // 念のためクリア

                // CSVファイルの内容をDataSetに入れる
                OleDbDataAdapter adapter = new OleDbDataAdapter(command);
                adapter.Fill(dataset);
                */

                // 読み込んだ内容を表示してみる
                foreach (DataTable table in dataset.Tables)
                {
                    ////▼▼▼DataTableのSelectメソッドを利用する。                    
                    DataRow[] dataRows = table.Select("[QuoteNumber__QT] = '" + readedCsvCol[31] + "'");
                    for (int i = 0; i < dataRows.Length; i++)
                    {
                        //Console.WriteLine(dataRows[i][1]);
                        if (readedCsvCol[31].Equals(dataRows[i][31]))          //csv.試算番号     = DB.試算番号
                            if (readedCsvCol[57].Equals(dataRows[i][60]))      //csv.親品目コード = DB.品目コード
                                if (DateTime.Parse(readedCsvCol[11]).Equals(dataRows[i][11]))  //csv.契約有効日   = DB.契約有効日 : リハ２にて明細CSVに同一試算番号、品目コードのデータがあったので
                                {
                                    subscription.ParentSubscriptionNumber__c = (string)dataRows[i][8];
                                    break;
                                }
                    }                    
                }

                //subscription.Notes = "memoUpted";
                string subscriptionKey = readedCsvCol[8];   //Subscription number
                return subscriptionsApi.PUTSubscription(subscriptionKey, subscription, "211.0");
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("updateOyaSubscriptionNo() failed with message: " + ex.Message);
                PUTSubscriptionResponseType localVarResponse = null;
                return localVarResponse;
            }
        }

        //
        //★★ READ csv file to create Subscription        
        public PUTSubscriptionResponseType updateSubscrWithCsv(string[] readedCsvCol)
        {
            try
            {
                ApplicationManager manager = new ApplicationManager(userID, passWD);
                //Initialize the Subscription container

                PUTSubscriptionType subscription = new PUTSubscriptionType();                               
                
                string subscriptionKey = readedCsvCol[8];   //Subscription number
                //AccountID取得    
                /** ２０件しか取得できない（API制限）ので、「Get subscriptions by key」に変更            
                string accountKey = null;
                if (!string.IsNullOrEmpty(readedCsvCol[9]))
                {
                    accountKey = readedCsvCol[9];
                    //CSVのaccountKeyがアカウント名だったらAccountIdを取得する。                    
                    ProxyActionqueryRequest zAccount = new ProxyActionqueryRequest();
                    zAccount.QueryString = "select id from account where name = '" + accountKey + "'";
                    string accountId = actionCreateApi.ProxyActionPOSTquery(zAccount);
                    if (!string.IsNullOrEmpty(accountId))
                    {
                        accountKey = accountId;
                    }
                }
                */
                //AccountIDで「Get subscriptions by account」、ratePlanNameで該当のsubscriptionを選定 
                /** ２０件しか取得できない（API制限）ので、「Get subscriptions by key」に変更
                GETSubscriptionWrapper subAccount = subscriptionsApi.GETSubscription(accountKey, chargeDetail);
                List<GETSubscriptionType> subscriptions = subAccount.Subscriptions;
                foreach (GETSubscriptionType ss in subscriptions)
                {
                    List<GETSubscriptionRatePlanType> RatePlans = ss.RatePlans;
                    foreach (GETSubscriptionRatePlanType rp in RatePlans)
                    {
                        if (ratePlanName.Equals(rp.RatePlanName))
                        {
                            Console.Out.WriteLine("Rate Plan found: " + rp.ToString());
                            ratePlanId = rp.Id;                        }                    }
                }
                */
                /**  makeSubscriptionCSV()にてGETOneSubscription()変更_string[]_してるので取り合えずコメント化（削除してもいいかも）
                GETSubscriptionTypeWithSuccess subOne = subscriptionsApi.GETOneSubscription(subscriptionKey, chargeDetail);
                List<GETSubscriptionRatePlanType> RatePlans = subOne.RatePlans;
                foreach (GETSubscriptionRatePlanType rp in RatePlans)
                {
                    if (ratePlanName.Equals(rp.RatePlanName))
                    {
                        Console.Out.WriteLine("Rate Plan found: " + rp.ToString());
                        ratePlanId = rp.Id;
                        //★★★ratePlanChargeIdも～～～▶▶▶
                    }
                }
                */

                //set the fields with CSV file columns

                // ST_ADD KMIBS-25  4．請求先の変更
                // Amendで請求先変更する。
                // 5_invoiceOwnerAccountId:請求先IDを全て消した状態で、変更する請求先のみを編集してもらう
                if (!string.IsNullOrEmpty(readedCsvCol[5]))
                {
                    ProxyActionqueryRequest zAccount = new ProxyActionqueryRequest();
                    zAccount.QueryString = "select id from account where HARISCostomerNumber__c = '" + readedCsvCol[5] + "'";
                    string invoiceOwnerAccountId = actionCreateApi.ProxyActionPOSTquery(zAccount);

                    if (!string.IsNullOrEmpty(invoiceOwnerAccountId))
                    {
                        //Initialize the container
                        ProxyActionamendRequest amendRequest = new ProxyActionamendRequest();
                        List<AmendRequest> RequestsList = new List<AmendRequest>();
                        AmendRequest zChgAmendRequest = new AmendRequest();

                        AmendOptions amendOptions = new AmendOptions();
                        amendOptions.GenerateInvoice = false;
                        amendOptions.ProcessPayments = false;
                        //amendOptions.InvoiceProcessingOptions = { };
                        List<Amendment> amendmentsList = new List<Amendment>();
                        Amendment amendMents = new Amendment();
                        amendMents.SubscriptionId = readedCsvCol[1];
                        amendMents.Name = readedCsvCol[8] + " : AmendForUpdateInvoiceID";
                        amendMents.Type = "OwnerTransfer";
                        amendMents.ContractEffectiveDate = DateTime.Parse(readedCsvCol[11]);
                        amendMents.DestinationInvoiceOwnerId = invoiceOwnerAccountId;
                        amendmentsList.Add(amendMents);

                        zChgAmendRequest.AmendOptions = amendOptions;
                        zChgAmendRequest.Amendments = amendmentsList;
                        RequestsList.Add(zChgAmendRequest);
                        amendRequest.Requests = RequestsList;
                        
                        //ProxyActionamendResponse updateInvoiceID = actionCreateApi.ProxyActionPOSTamend(amendRequest);
                        string resultUpdateInvoiceID = actionCreateApi.ProxyActionPOSTamendString(amendRequest);
                        Console.Out.WriteLine("resultUpdateInvoiceID " + resultUpdateInvoiceID );                        
                    }
                    else
                        Console.Out.WriteLine("更新用の請求先を確認してください。 ");
                }
                // ED_ADD KMIBS-25

                // ST_ADD KMIBS-51  納入先の変更
                // Amendで納入先を変更する。
                // 2_accountId:納入先IDを全て消した状態で、変更する納入先のみを編集してもらう
                if (!string.IsNullOrEmpty(readedCsvCol[2]))
                {
                    ProxyActionqueryRequest zAccount = new ProxyActionqueryRequest();
                    zAccount.QueryString = "select id from account where HARISCostomerNumber__c = '" + readedCsvCol[2] + "'";
                    string changeAccountId = actionCreateApi.ProxyActionPOSTquery(zAccount);

                    if (!string.IsNullOrEmpty(changeAccountId))
                    {
                        //Initialize the container
                        ProxyActionamendRequest amendRequest = new ProxyActionamendRequest();
                        List<AmendRequest> RequestsList = new List<AmendRequest>();
                        AmendRequest zChgAmendRequest = new AmendRequest();

                        AmendOptions amendOptions = new AmendOptions();
                        amendOptions.GenerateInvoice = false;
                        amendOptions.ProcessPayments = false;
                        //amendOptions.InvoiceProcessingOptions = { };
                        List<Amendment> amendmentsList = new List<Amendment>();
                        Amendment amendMents = new Amendment();
                        amendMents.SubscriptionId = readedCsvCol[1];
                        amendMents.Name = readedCsvCol[8] + " : AmendForChangeAccountID";
                        amendMents.Type = "OwnerTransfer";
                        amendMents.ContractEffectiveDate = DateTime.Parse(readedCsvCol[11]);
                        amendMents.DestinationAccountId = changeAccountId;
                        amendmentsList.Add(amendMents);

                        zChgAmendRequest.AmendOptions = amendOptions;
                        zChgAmendRequest.Amendments = amendmentsList;
                        RequestsList.Add(zChgAmendRequest);
                        amendRequest.Requests = RequestsList;

                        //ProxyActionamendResponse updateInvoiceID = actionCreateApi.ProxyActionPOSTamend(amendRequest);
                        string resultChangeAccountID = actionCreateApi.ProxyActionPOSTamendString(amendRequest);
                        Console.Out.WriteLine("resultChangeAccountID " + resultChangeAccountID);
                    }
                    else
                        Console.Out.WriteLine("更新用の納入先を確認してください。 ");
                }
                // ED_ADD KMIBS-51

                // 9_termType
                if (!string.IsNullOrEmpty(readedCsvCol[9]))  
                    subscription.TermType = readedCsvCol[9];
                // 15_termStartDate
                if (!string.IsNullOrEmpty(readedCsvCol[15]))
                    subscription.TermStartDate = manager.convertToDate(readedCsvCol[15]);
                // 19_currentTerm                
                if (!string.IsNullOrEmpty(readedCsvCol[19]))
                    subscription.CurrentTerm = long.Parse(readedCsvCol[19]);
                // 20_currentTermPeriodType
                if (!string.IsNullOrEmpty(readedCsvCol[20]))
                    subscription.CurrentTermPeriodType = readedCsvCol[20];
                // 21_autoRenew
                if (!string.IsNullOrEmpty(readedCsvCol[21]))
                    subscription.AutoRenew = bool.Parse(readedCsvCol[21]);
                // 22_renewalSetting
                if (!string.IsNullOrEmpty(readedCsvCol[22]))
                    subscription.RenewalSetting = readedCsvCol[22];
                // 23_renewalTerm
                if (!string.IsNullOrEmpty(readedCsvCol[23]))
                    subscription.RenewalTerm = long.Parse(readedCsvCol[23]);
                // 27_notes
                if (!string.IsNullOrEmpty(readedCsvCol[27]))
                    subscription.Notes = readedCsvCol[27];
                // 31_ParentSubscriptionNumber__c -> 33_ParentSubscriptionNumber__c
                if (!string.IsNullOrEmpty(readedCsvCol[33]))
                    subscription.ParentSubscriptionNumber__c = readedCsvCol[33];
                // 35_AutoRenewVersion__c -> 40_AutoRenewVersion__c
                if (!string.IsNullOrEmpty(readedCsvCol[40]))
                    subscription.AutoRenewVersion__c = readedCsvCol[40];
                // 36_ServiceStartDate__c -> 44_ServiceStartDate__c
                //なぜかDateTimeではエラーになるので、文字列の"YYYY-MM-DD"とする
                if (!string.IsNullOrEmpty(readedCsvCol[44]))
                { 
                    //subscription.ServiceStartDate__c = manager.convertToDate(readedCsvCol[36]);
                    //subscription.ServiceStartDate__c = readedCsvCol[36];
                    DateTime dateKaishi = DateTime.Parse(readedCsvCol[44]);
                    string kaisiDate = dateKaishi.ToString("yyyy-MM-dd");
                    subscription.ServiceStartDate__c = kaisiDate;
                }
                // 41_NeTTSSyncFlag__c -> 49_NeTTSSyncFlag__c
                if (!string.IsNullOrEmpty(readedCsvCol[49]))
                    subscription.NeTTSSyncFlag__c = readedCsvCol[49];

                //add st KMIBS-107
                DateTime dateWork;
                string dateWorkStr;
                // 利用許諾申し込み日, 35_LicenseOfferDate__c
                if (!string.IsNullOrEmpty(readedCsvCol[35]))
                {   
                    dateWork = DateTime.Parse(readedCsvCol[35]);
                    dateWorkStr = dateWork.ToString("yyyy-MM-dd");
                    subscription.LicenseOfferDate__c = dateWorkStr;
                }
                // お試し申し込み日,   41_TrialOfferDate__c
                if (!string.IsNullOrEmpty(readedCsvCol[41]))
                {
                    dateWork = DateTime.Parse(readedCsvCol[41]);
                    dateWorkStr = dateWork.ToString("yyyy-MM-dd");
                    subscription.TrialOfferDate__c = dateWorkStr;
                }
                // 本申し込み日,       29_OfferDate__c
                if (!string.IsNullOrEmpty(readedCsvCol[29]))
                {
                    dateWork = DateTime.Parse(readedCsvCol[29]);
                    dateWorkStr = dateWork.ToString("yyyy-MM-dd");
                    subscription.OfferDate__c = dateWorkStr;
                }
                // 課金開始日,         36_BillingStartDate__c
                if (!string.IsNullOrEmpty(readedCsvCol[36]))
                {
                    dateWork = DateTime.Parse(readedCsvCol[36]);
                    dateWorkStr = dateWork.ToString("yyyy-MM-dd");
                    subscription.BillingStartDate__c = dateWorkStr;
                }
                //add ed KMIBS-107

                //★■ ratePlanId,ratePlanChargeId取得                
                //Initialize the Rate Plan container list (Must use a list as the subscription can have multiple rate plans)
                List<PUTSrpUpdateType> ratePlanList = new List<PUTSrpUpdateType>();
                //Initialize the individual Rate Plan container
                PUTSrpUpdateType ratePlan = new PUTSrpUpdateType();

                //●Populate the Rate Plan container with the required field
                /** Amend前のIDでも更新されるので、ratePlanIDは最新で、chargeIDは古いモノで試す->できる
                ratePlan.RatePlanId = ratePlanId;
                */
                // subscription ratePlanID(44_id -> 43_id -> 51_id)
                ratePlan.RatePlanId = readedCsvCol[51];

                //11_contractEffectiveDate : Required
                if (!string.IsNullOrEmpty(readedCsvCol[11]))
                    ratePlan.ContractEffectiveDate = manager.convertToDate(readedCsvCol[11]);                                
                //12_ serviceActivationDate
                if (!string.IsNullOrEmpty(readedCsvCol[12]))
                    ratePlan.ServiceActivationDate = manager.convertToDate(readedCsvCol[12]);
                //13_ customerAcceptanceDate
                if (!string.IsNullOrEmpty(readedCsvCol[13]))
                    ratePlan.CustomerAcceptanceDate = manager.convertToDate(readedCsvCol[13]);                

                /**  PUTSrpUpdateTypeにない項目
                //14_ subscriptionStartDate
                if (!string.IsNullOrEmpty(readedCsvCol[14]))
                {
                    ratePlan.SubscriptionStartDate = manager.convertToDate(readedCsvCol[14]);
                }//15_ termStartDate
                if (!string.IsNullOrEmpty(readedCsvCol[15]))
                {
                    ratePlan.TermStartDate = manager.convertToDate(readedCsvCol[15]);
                }//16_ termEndDate
                if (!string.IsNullOrEmpty(readedCsvCol[16]))
                {
                    ratePlan.TermEndDate = manager.convertToDate(readedCsvCol[16]);
                }*/

                //ratePlanCharge▼▼▼
                //Initialize the Rate Plan CHARGE container list (Must use a list as the subscription can have multiple rate plans)
                List<PUTScUpdateType> ratePlanChargeList = new List<PUTScUpdateType>();
                //Initialize the individual Rate Plan container
                PUTScUpdateType ratePlanCharge = new PUTScUpdateType();
                //●Populate the Rate Plan Charge container with the required field                               
                ratePlanCharge.RatePlanChargeId = readedCsvCol[65];  // ratePlanChargeId (55_id -> 56_id -> 65_id)

                // del st KMIBS-142
                ////add st KMIBS-107 利用許諾のないサブスクリプションの金額をゼロにする
                ////課金対象フラグ(58_BillingFlag__c) = TRUE 利用許諾要否フラグ（61_RequiredLicense__c）＝FALSE 以外は価格をZeroにする
                //bool goZero = false;
                ////if (readedCsvCol[58].Equals("true") && readedCsvCol[61].Equals("false") || readedCsvCol[58].Equals("TRUE") && readedCsvCol[61].Equals("FALSE"))
                //if (bool.Parse(readedCsvCol[58]).Equals(true) && bool.Parse(readedCsvCol[61]).Equals(false))
                //    goZero = false;
                //else
                //    goZero = true;
                ////add ed KMIBS-107
                // del ed KMIBS-142

                if (!string.IsNullOrEmpty(readedCsvCol[78])) { 
                    ratePlanCharge.Price = readedCsvCol[78]; //68_price -> 69_price -> 78_price
                    // del st KMIBS-142
                    ////add st KMIBS-107 利用許諾のないサブスクリプションの金額をゼロにする                    
                    //if (goZero)
                    //        ratePlanCharge.Price = "0";
                    ////add ed KMIBS-107
                    // del ed KMIBS-142
                }

                //■●★ フィールド「quantity」は課金モデルがFlat Fee Pricingの課金に無効です
                // 60->61_type -> 70_type =Recurring  61->62_model -> 71_model = PerUnit のみ数量変更可能 
                //if (!readedCsvCol[61].Equals("FlatFee"))
                if (readedCsvCol[70].Equals("Recurring") && readedCsvCol[71].Equals("PerUnit"))
                    if (!string.IsNullOrEmpty(readedCsvCol[93]))
                        ratePlanCharge.Quantity = readedCsvCol[93]; //83->84_quantity -> 93_quantity

                // 66_priceChangeOption -> 75_priceChangeOption
                if (!string.IsNullOrEmpty(readedCsvCol[75]))
                    ratePlanCharge.PriceChangeOption = readedCsvCol[75];
                // 67_priceIncreasePercentage -> 76_priceIncreasePercentage
                if (!string.IsNullOrEmpty(readedCsvCol[76]))
                    ratePlanCharge.PriceIncreasePercentage = readedCsvCol[76];
                // 83_billingPeriodAlignment -> 92_billingPeriodAlignment
                if (!string.IsNullOrEmpty(readedCsvCol[92]))
                    ratePlanCharge.BillingPeriodAlignment = readedCsvCol[92];
                // 71_includedUnits -> 80_includedUnits
                if (!string.IsNullOrEmpty(readedCsvCol[80]))
                    ratePlanCharge.IncludedUnits = readedCsvCol[80];
                // 72_overagePrice -> 81_overagePrice
                if (!string.IsNullOrEmpty(readedCsvCol[81])) { 
                    ratePlanCharge.OveragePrice = readedCsvCol[81];
                    // del st KMIBS-142
                    ////add st KMIBS-107 利用許諾のないサブスクリプションの金額をゼロにする
                    //if (goZero)
                    //    ratePlanCharge.OveragePrice = "0";
                    ////add ed KMIBS-107
                    // del ed KMIBS-142
                }

                // 97_triggerDate -> 106_triggerDate
                if (!string.IsNullOrEmpty(readedCsvCol[106]))
                    ratePlanCharge.TriggerDate = manager.convertToDate(readedCsvCol[106]);

                // < UCE > ContractEffective, < USA > ServiceActivation,< UCA > CustomerAcceptance , < USD > SpecificDate                
                // 98_triggerEvent -> 107_triggerEvent
                if (!string.IsNullOrEmpty(readedCsvCol[107]))
                {
                    if (readedCsvCol[98].Equals("ContractEffective"))
                        ratePlanCharge.TriggerEvent = "UCE";
                    else if (readedCsvCol[98].Equals("ServiceActivation"))
                        ratePlanCharge.TriggerEvent = "USA";
                    else if (readedCsvCol[98].Equals("CustomerAcceptance"))
                        ratePlanCharge.TriggerEvent = "UCA";
                    else if (readedCsvCol[98].Equals("SpecificDate"))
                    {
                        ratePlanCharge.TriggerEvent = "USD";
                        if (!string.IsNullOrEmpty(readedCsvCol[106]))
                        {
                            DateTime dateCsv = DateTime.Parse(readedCsvCol[106]);
                            ratePlanCharge.TriggerDate = dateCsv;
                        }
                    }
                }

                // 107_description -> 116_description
                if (!string.IsNullOrEmpty(readedCsvCol[116]))
                    ratePlanCharge.Description = readedCsvCol[116];
                // 108_UserPrice__c -> 118_UserPrice__c
                if (!string.IsNullOrEmpty(readedCsvCol[118]))
                    ratePlanCharge.UserPrice__c = readedCsvCol[118];

                //add st KMIBS-107
                // 117_UserPriceTotal__c
                if (!string.IsNullOrEmpty(readedCsvCol[117]))
                    ratePlanCharge.UserPriceTotal__c = readedCsvCol[117];
                // 119_EffectivePriceTotal__c
                if (!string.IsNullOrEmpty(readedCsvCol[119]))
                    ratePlanCharge.EffectivePriceTotal__c = readedCsvCol[119];
                //add ed KMIBS-107

                //ratePlanChargeTier ▼▼▼▼▼
                if (!string.IsNullOrEmpty(readedCsvCol[79]))  // 69->70_tiers->79_tiers=nullなら 112->110_tier->121_tierのデータはない。。。
                {
                    /** // Tier,Priceだけなら、複数のTierが更新できるが、他を設定するとTier[1] 更新にてTier[2]が削除される。。。    
                        =>  //●■ 20170720 TierのCSV編集を２行→１行にする。
                    */
                    List<POSTTierType> ratePlanChargeTierList = new List<POSTTierType>();
                    long lengthCsvCol = readedCsvCol.Length;
                    for (int i = 0; (121 + 6 * i) < lengthCsvCol; i++ ) { 
                        //Initialize the Rate Plan CHARGE container list (Must use a list as the subscription can have multiple rate plans)
                        //List<POSTTierType> ratePlanChargeTierList = new List<POSTTierType>();
                        //Initialize the individual Rate Plan container
                        POSTTierType ratePlanChargeTier = new POSTTierType();
                        //Populate the Rate Plan container with the required field                
                        if (!string.IsNullOrEmpty(readedCsvCol[121 + 6 * i]))   //112_tier -> 110_tier -> 121_tier
                            ratePlanChargeTier.Tier = long.Parse(readedCsvCol[121 + 6 * i]);                                       
                        if (!string.IsNullOrEmpty(readedCsvCol[122 + 6 * i]))   //113_startingUnit -> 111_startingUnit -> 122_startingUnit
                            ratePlanChargeTier.StartingUnit = readedCsvCol[122 + 6 * i];
                        if (!string.IsNullOrEmpty(readedCsvCol[123 + 6 * i]))  //114_endingUnit -> 112_endingUnit -> 123_endingUnit
                            ratePlanChargeTier.EndingUnit = readedCsvCol[123 + 6 * i];
                        if (!string.IsNullOrEmpty(readedCsvCol[124 + 6 * i])) //115_price -> 113_price -> 124_price
                        {   
                            ratePlanChargeTier.Price = readedCsvCol[124 + 6 * i];
                            // del st KMIBS-142
                            ////add st KMIBS-107 利用許諾のないサブスクリプションの金額をゼロにする
                            //if (goZero)
                            //    ratePlanChargeTier.Price = "0";
                            ////add ed KMIBS-107
                            // del ed KMIBS-142
                        }
                        if (!string.IsNullOrEmpty(readedCsvCol[125 + 6 * i]))  //116_priceFormat -> 114_priceFormat -> 125_priceFormat
                            ratePlanChargeTier.PriceFormat = readedCsvCol[125 + 6 * i];
                                                
                        // 20170828 Excel編集にて、Tier1のみのデータにTier2のNULLができてしまうので
                        if (!string.IsNullOrEmpty(readedCsvCol[121 + 6 * i]))   //112_tier -> 110_tier -> 121_tier
                            //Add the individual Rate Plan container to the list
                            ratePlanChargeTierList.Add(ratePlanChargeTier);                        
                    }
                    //Add the list of Rate Plans to the Subscription container
                    ratePlanCharge.Tiers = ratePlanChargeTierList;
                }
                //ratePlanCharge▲▲▲▲▲

                //Add the individual Rate Plan container to the list
                ratePlanChargeList.Add(ratePlanCharge);
                //Add the list of Rate Plans to the Subscription container
                ratePlan.ChargeUpdateDetails = ratePlanChargeList;
                //ratePlanCharge▲▲▲

                //Add the individual Rate Plan container to the list
                ratePlanList.Add(ratePlan);

                //Add the list of Rate Plans to the Subscription container
                subscription.Update = ratePlanList;
                
                return subscriptionsApi.PUTSubscription(subscriptionKey, subscription, "211.0");
            }
            catch (Exception ex)
            {
                Console.WriteLine("updateSubscrWithCsv() failed with message: " + ex.Message);
                PUTSubscriptionResponseType localVarResponse = null;
                return localVarResponse;
            }
        }

        private DateTime convertToDate(string strDate)
        {
            //20170828 yyyy/mm/ddの対応：yyyy-mm-dd変換する
            //DateTime dateConverted = DateTime.Parse(strDate);
            //return dateConverted;
            DateTime dateWork = DateTime.Parse(strDate);
            string strWork = dateWork.ToString("yyyy-MM-dd");

            DateTime dateConverted = DateTime.Parse(strWork);
            return dateConverted;
        }

        /**
         * Method for creating a Zuora Account and Subscription with the given product
         * 
         * prodId: The ID of the product to be added to the new subscription
         */
        //★ READ csv file
        //public POSTAccountResponseType createAccountAndSub(string prodId)
        public POSTAccountResponseType createAccountAndSub(string prodId, string[] readedCsvCol)
        {
            //Initialize the Account container
            POSTAccountType zAcc = new POSTAccountType();

            //Initialize the Bill-To Contact container
            POSTAccountTypeBillToContact bill2Contact = new POSTAccountTypeBillToContact();

            //Populate the Bill-To Contact with all required fields            
            //★ READ csv file
            /**
            bill2Contact.FirstName = "John";
            bill2Contact.LastName = "Doe";
            bill2Contact.Country = "japan";
            bill2Contact.State = "Georgia";
            */            
            if (!string.IsNullOrEmpty(readedCsvCol[0]))
            {
                bill2Contact.FirstName = readedCsvCol[0];
            }
            if (!string.IsNullOrEmpty(readedCsvCol[1]))
            {
                bill2Contact.LastName = readedCsvCol[1];
            }
            if (!string.IsNullOrEmpty(readedCsvCol[2]))
            {
                bill2Contact.Country = readedCsvCol[2];
            }
            if (!string.IsNullOrEmpty(readedCsvCol[3]))
            {
                bill2Contact.State = readedCsvCol[3];
            }

            //Initialize the Sold-To Contact container
            POSTAccountTypeSoldToContact sold2Contact = new POSTAccountTypeSoldToContact();

            //Populate the Sold-To Contact with all required fields
            sold2Contact.FirstName = "John";
            sold2Contact.LastName = "Doe";
            sold2Contact.Country = "Korea, Republic of";
            sold2Contact.State = "Georgia";

            //Initialize the Credit Card container
            POSTAccountTypeCreditCard creditCard = new POSTAccountTypeCreditCard();

            //Initialize the Card-Holder Information container
            POSTAccountTypeCreditCardCardHolderInfo info = new POSTAccountTypeCreditCardCardHolderInfo();

            //Populate the Card-Holder Information with all required fields
            info.CardHolderName = "John Doe";
            info.AddressLine1 = "3525 Piedmont Road";
            info.City = "Atlanta";
            //★info.City = "USA";
            info.Country = "USA";
            info.State = "GA";
            info.ZipCode = "30305";

            //Set the Card-Holder Information on the Credit Card container
            creditCard.CardHolderInfo = info;

            //Populate the Credit Card with all required fields
            creditCard.CardType = "Visa";
            creditCard.CardNumber = "4111111111111111";
            creditCard.ExpirationMonth = "10";
            creditCard.ExpirationYear = "2020";
            creditCard.SecurityCode = "111";

            //Initialize the Subscription container
            POSTAccountTypeSubscription subscription = new POSTAccountTypeSubscription();

            //Initialize the Rate Plan container list (Must use a list as the subscription can have multiple rate plans)
            List<POSTSrpCreateType> ratePlanList = new List<POSTSrpCreateType>();

            //Initialize the individual Rate Plan container
            POSTSrpCreateType ratePlan = new POSTSrpCreateType();

            //Populate the Rate Plan container with the required field
            ratePlan.ProductRatePlanId = prodId;

            //Add the individual Rate Plan container to the list
            ratePlanList.Add(ratePlan);

            //Add the list of Rate Plans to the Subscription container
            subscription.SubscribeToRatePlans = ratePlanList;

            //Populate the subscription with all other required fields
            subscription.TermType = "TERMED";
            subscription.AutoRenew = false;
            subscription.InitialTerm = 12;
            subscription.RenewalTerm = 12;
            //★subscription.ContractEffectiveDate = new DateTime(2016, 10, 17);
            subscription.ContractEffectiveDate = new DateTime(2017, 07, 01);

            //Add the Bill-To Contact container to the Account
            zAcc.BillToContact = bill2Contact;

            //Add the Sold-To Contact container to the Account
            zAcc.SoldToContact = sold2Contact;

            //Add the Credit Card container to the Account
            //★ TEST without Credit Card Info->fail to create account and...            
            zAcc.CreditCard = creditCard;

            //Add the Subscription container to the Account
            zAcc.Subscription = subscription;

            //Populate all other required fields on the Account
            //★zAcc.Name = "Test Account";
            //★zAcc.Currency = "USD";

            //★ READ csv file
            //zAcc.Name = "ikoGitHubDelPaymMethod";            
            if (!string.IsNullOrEmpty(readedCsvCol[4]))
            {
                zAcc.Name = readedCsvCol[4];
            }

            zAcc.Currency = "jpy";

            //★ 
            zAcc.AutoPay = false;

            //★★★API仕様上ないFieldの追加試み -> NG
            //zAcc.ParentId = "2c92c0fa5c48f2f2015c51c76a0a2b04";

            //Submit the API call by passing in the required Account container and the API version
            //return accountsApi.POSTAccount(zAcc, "196.0");
            return accountsApi.POSTAccount(zAcc, "211.0");
        }

        /**
         * Method for upgading a subscription
         * This method takes a given subscription and removes the rate plan specified for removal and adds the product 
         * rate plan specified for addition
         * 
         * subNum: The Subscription Number of the subscription to be changed
         * removeRpId: The ID of the Rate Plan to be removed from the subscription
         * addPrpId: The ID of the Product Rate Plan to be added to the subscription
         */
        public PUTSubscriptionResponseType upgradeSubscription(string subNum, string removeRpId, string addPrpId)
        {
            //Initialize the container for the new version of the subscription
            PUTSubscriptionType updatedSub = new PUTSubscriptionType();

            //Initialize the list of product rate plans to be added to the subscription
            List<PUTSrpAddType> ratePlanList = new List<PUTSrpAddType>();

            //Initalize the container for the added Subscription Rate Plan
            PUTSrpAddType ratePlan = new PUTSrpAddType();

            //Populate all required fields on the Subscription Rate Plan
            ratePlan.ProductRatePlanId = addPrpId;
            ratePlan.ContractEffectiveDate = new DateTime(2016, 10, 25);

            //Add the Subscription Rate Plan container to the list
            ratePlanList.Add(ratePlan);

            //Set the list of Subscription Rate Plans to be added on the subscription
            updatedSub.Add = ratePlanList;

            //Initialize the list of subscription rate plans to be removed from the subscription
            List<PUTSrpRemoveType> removeRatePlanList = new List<PUTSrpRemoveType>();

            //Initialize the container for the Subscription Rate Plan to be removed
            PUTSrpRemoveType removeRatePlan = new PUTSrpRemoveType();

            //Populate all required fields on the Subscription Rate Plan
            removeRatePlan.RatePlanId = removeRpId;
            removeRatePlan.ContractEffectiveDate = new DateTime(2016, 10, 25);

            //Add the Subscription Rate Plan to the list
            removeRatePlanList.Add(removeRatePlan);

            //Set the list of Subscription Rate Plans to be removed from the subscription
            updatedSub.Remove = removeRatePlanList;

            //Submit the API call by passing in the required Subscription Key(Subscription Number), Subscription container, and API version
            return subscriptionsApi.PUTSubscription(subNum, updatedSub, "196.0");
        }

        /**
         * Method for cancelling a subscription
         * 
         * subNum: The number of the subscription to be cancelled
         */
        public POSTSubscriptionCancellationResponseType cancelSubscription(string subNum)
        {
            //Initialize the Cancel Subscription request container
            POSTSubscriptionCancellationType cancelSub = new POSTSubscriptionCancellationType();

            //Populate the container with all required fields
            cancelSub.CancellationPolicy = "SpecificDate";
            cancelSub.Invoice = false;
            cancelSub.CancellationEffectiveDate = new DateTime(2016, 11, 3);

            //Submit the API call by passing in the required Subscription Key(Subscription Number), Cancel Request container, and API version
            return subscriptionsApi.POSTSubscriptionCancellation(subNum, cancelSub, "196.0");
        }

        /** ★★★
        * Method for PaymentMethod  BankInfo
        */
        public ProxyActioncreateResponse createPayMethodBank(string accountId)
        {
            //Initialize the container
            ProxyActioncreateRequestBank zPay = new ProxyActioncreateRequestBank();

            //List Of Column for Adding Bank Info to PaymentMethod
            List<ZObjectPayBank> payMentBankList = new List<ZObjectPayBank>();

            //Initialize the PaymentMethod container
            ZObjectPayBank zPayMentBank = new ZObjectPayBank();

            //Bank Info用のカラムを指定してリストとして格納する。
            zPayMentBank.AccountId = accountId;
            zPayMentBank.BankName = "ikoRestBank";
            zPayMentBank.Type = "BankTransfer";
            zPayMentBank.BankCode = "0001";
            zPayMentBank.PaymentRetryWindow = 1;
            zPayMentBank.UseDefaultRetryRule = false;
            zPayMentBank.BankTransferType = "SEPA";
            zPayMentBank.BankTransferAccountNumber = "00108127890";
            //zPayMentBank.PaymentMethodStatus = "Closed";

            payMentBankList.Add(zPayMentBank);

            zPay.Objects = payMentBankList;
            zPay.Type = "PaymentMethod";

            return actionCreateApi.ProxyActionPOSTcreate(zPay);
        }

        /** ★★★
        * Update Default Payment MethodID with  BankInfoID by ActionApi
        * NG1
        */
        public ProxyActionupdateResponse updatePayMethodId(string accountId, string payMentIdBank)
        {
            //Initialize the container
            ProxyActionupdateRequestDefPay zDefPayID = new ProxyActionupdateRequestDefPay();

            //List Of Column for Adding Bank Info to PaymentMethod
            List<ZObjectPayDef> defPayDefIdList = new List<ZObjectPayDef>();

            //Initialize the PaymentMethod container
            ZObjectPayDef zPayDef = new ZObjectPayDef();

            //Bank Info用のカラムを指定してリストとして格納する。
            zPayDef.Id = accountId;
            //zPayDef.DefaultpaymentMethodId = payMentIdBank;
            //zPayDef.Name = "ikoGitHubPaymMethod_Bank";
            //zPayDef.Notes = "#####note#####";

            defPayDefIdList.Add(zPayDef);

            zDefPayID.Objects = defPayDefIdList;
            zDefPayID.Type = "Account";

            return actionCreateApi.ProxyActionPOSTupdate(zDefPayID);
        }

        /** ★★★
        * Action>Updateでは「DefaultPaymentMethodId」が更新できないので
        * PUT /payment-methods/credit-cards/{payment-method-id}にて更新する。
        * cause it is not credit card, Unable to update the defaultPaymentMethod
        * NG2
        */
        public PUTPaymentMethodResponseType putPayMethodIdWithBank(string payMethodIdBank)
        {
            //Initialize the container
            PUTPaymentMethodType zDefPayIDbank = new PUTPaymentMethodType();

            zDefPayIDbank.DefaultPaymentMethod = true;

            return paymentMethodsApi.PUTPaymentMethods(payMethodIdBank, zDefPayIDbank);
        }

        /** ★★★
        * 三度目の正直
        * public ProxyCreateOrModifyResponse ProxyPUTAccount (string id, ProxyModifyAccount modifyRequest)
        * accountPayMethodBankUpdateCRUD
        */
        public ProxyCreateOrModifyResponse accountPayMethodBankUpdateCRUD(string accountId, string payMentIdBank)
        {
            //Initialize the container
            ProxyModifyAccount zDefPayIDbank = new ProxyModifyAccount();

            zDefPayIDbank.DefaultPaymentMethodId = payMentIdBank;

            return accountsApi.ProxyPUTAccount(accountId, zDefPayIDbank);
        }

        /** //★★paymentMethodを削除する試みで、AutoPayをfalseにしてDefaultPaymentMethodIDをNULLにしてみる
        // zDefPayIDbank.DefaultPaymentMethodId = null;　は、更新しない意味 
        public ProxyCreateOrModifyResponse accountPayMethodIdNullUpdateCRUD(string accountId, string payMentIdCredit)
        {
            //Initialize the container
            ProxyModifyAccount zDefPayIDbank = new ProxyModifyAccount();

            zDefPayIDbank.AutoPay = false;
            zDefPayIDbank.DefaultPaymentMethodId = payMentIdCredit;            

            return accountsApi.ProxyPUTAccount(accountId, zDefPayIDbank);
        }  
        */

        /** ★★★
        * select PaymentMethod  BankInfo
        */
        //public ProxyActionqueryResponse selectPayMethodBank(string accountId)
        public string selectPayMethodBank(string accountId)
        {
            //Initialize the container
            ProxyActionqueryRequest zPayBank = new ProxyActionqueryRequest();

            zPayBank.QueryString = "select id from PaymentMethod where AccountId = '" + accountId + "' and Type = 'BankTransfer'";

            return actionCreateApi.ProxyActionPOSTquery(zPayBank);
        }

        /** ★★★
        * Method for delete PaymentMethod 
        * 
        * PaymentMethodId: The number of the paymentMethod to be cancelled
        */
        public CommonResponseType deletePaymentMethod(string paymentMethodID)
        {
            return paymentMethodsApi.DELETEPaymentMethods(paymentMethodID);
        }

        /**
         * Main method for executing the sample API calls in sequence
         */
        static void Main(string[] args)
        {
            ApplicationManager manager = new ApplicationManager(userID, passWD);
            
            if (args.Count() != 0)
            {
                //アカウント作成
                if (args[0] == "account")
                {
                    manager.createAccount();
                }
                //Subscription作成
                else if (args[0] == "subscription")
                {
                    manager.createSubscription();
                }
                //Subscription更新
                else if (args[0] == "subscriptionUpdate")
                {
                    manager.updateSubscription();
                }
                //Subscription-CSV作成
                else if (args[0] == "subscriptionMakeCSV")
                {
                    manager.makeSubscriptionCSV();
                }
                //Subscription削除
                else if (args[0] == "delete")
                {
                    manager.deleteSubscription();
                }
                // KMIBS-54 親subScription番号を更新登録する
                //update OyaSubscriptionNo
                else if (args[0] == "oya")
                {
                    manager.updateOya();
                }
                //#7 品目展開　件数確認
                else if (args[0] == "check")
                {
                    manager.checksubScriptionCount();
                }
            }
            else
            {
                Console.Out.WriteLine("処理オプションなし"); 
            }
            
        } //End of main()

        /** #1 create account
         * 
         */
        public void createAccount(){
            //★ Print Log Out
            // 出力先ファイル名 accountLog   追加書き込み true   文字コードSystem.Text.Encoding.GetEncoding("UTF-8")
            StreamWriter sw = new StreamWriter(accountLog, true, System.Text.Encoding.GetEncoding("UTF-8"));
            Console.SetOut(sw); // 出力先（Outプロパティ）を設定
            Console.Out.WriteLine("処理開始▶▶▶▶▶" + System.DateTime.Now); //処理開始
            //★ Print Log Out           

            //Must set the JSON serializer settings so that null values are not passed to the API
            //Passing null values will result in a failed API call
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            //Initialize the Application Manager which contains all methods and objects used for the QuickStart
            //★ TODO UPDATE WITH YOUR USERNAME AND PASSWORD FOR YOUR ZUORA TEST DRIVE TENANT
            ApplicationManager manager = new ApplicationManager(userID, passWD);

            //★ READ csv file
            string csvfile = @accountCSV;   //CSVファイル格納場所（実行ファイル同）
            TextFieldParser parser = new TextFieldParser(csvfile, Encoding.GetEncoding("UTF-8"));
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(","); // 区切り文字はコンマ

            int countCsv = 0;
            while (!parser.EndOfData)
            {
                string[] csvColumn = parser.ReadFields(); // 1行読み込み

                if (countCsv == 0)
                {
                    countCsv += 1;
                    continue;  //headerは処理しない
                }

                for (int n = 0; n < csvColumn.Length; n++)
                {
                    Console.Write(csvColumn[n] + ",");
                }
                Console.WriteLine();

                //Name of the initial product rate plan to be added in our new subscription
                //★ string productRatePlan1Name = "Quarterly Plan";
                //★★ READ csv file to create Subscription
                //string productRatePlan1Name = "basicApiPlan";
                string productRatePlan1Name = csvColumn[6];

                /**
                //Name of the "upgraded" product rate plan to be added to the upgraded subscription
                //★string productRatePlan2Name = "Annual Plan";
                string productRatePlan2Name = "実験プラン";
                */

                //Creating ojects to store the IDs of the above product rate plans
                string productRatePlan1Id = null;
                //★ string productRatePlan2Id = null;

                Console.Out.WriteLine("Calling Get Catalog >");
                //Use the GET Catalog call to retrieve the entire product catalog and store it in the catalog container
                GETCatalogType catalog = manager.catalogApi.GETCatalog();

                //Get the list of products from the catalog container
                List<GETProductType> products = catalog.Products;

                //Loop through all products to find the ones we will use
                foreach (GETProductType p in products)
                {
                    //Get the list of rate plans from the product container
                    List<GETProductRatePlanType> ratePlans = p.ProductRatePlans;

                    //Loop through all rate plans for this product
                    foreach (GETProductRatePlanType rp in ratePlans)
                    {
                        //If the initial product rate plan is identified...
                        if (productRatePlan1Name.Equals(rp.Name))
                        {
                            Console.Out.WriteLine("Product Rate Plan 1 found: " + rp.ToString());
                            //Store the ID for our initial product rate plan
                            productRatePlan1Id = rp.Id;
                        }
                        /** "Product Rate Plan 2
                        //If the upgraded product rate plan is identified...
                        if (productRatePlan2Name.Equals(rp.Name))
                        {
                            Console.Out.WriteLine("Product Rate Plan 2 found: " + rp.ToString());

                            //Store the ID for our upgraded product rate plan
                            productRatePlan2Id = rp.Id;
                        }
                        */

                    }
                }
                //Create an object to store the result of the Create Account call
                POSTAccountResponseType accResponse = null;

                //If the initial product rate plan was found...
                if (productRatePlan1Id != null)
                {   
                    Console.Out.WriteLine("Calling Create Account and Subscription >");
                    //Create an account and subscription using the initial product rate plan                    
                    //★ READ csv file
                    //accResponse = manager.createAccountAndSub(productRatePlan1Id);
                    accResponse = manager.createAccountAndSub(productRatePlan1Id, csvColumn);

                    Console.Out.WriteLine("POSTAccountResponseType: " + accResponse.ToString());
                }
                else
                {
                    Console.Out.WriteLine("Product Rate Plan 1, 'Quarterly Plan', was not found");
                }

                //★without Change rate plan
                /**
                //If the upgraded product rate plan was found...
                if (productRatePlan2Id != null)
                {
                    //If the create account call was successful...
                    if ((accResponse != null) && (accResponse.Success == true))
                    {
                        //Create an object to store the subscription rate plan
                        string existingRpId = null;

                        //Query for the subscription that was created to get the existing subscription rate plan using the GET One Subscription call
                        //This call takes in the Subscription key(Subscriptin Number) and the Charge Detail segments to be returned
                        GETSubscriptionTypeWithSuccess getSubResponse = manager.subscriptionsApi.GETOneSubscription(accResponse.SubscriptionNumber, "all-segments");

                        //Get the list of subscription rate plans from the Subscription response container
                        List<GETSubscriptionRatePlanType> ratePlans = getSubResponse.RatePlans;

                        //Loop through each rate plan in the existing subscription
                        foreach(GETSubscriptionRatePlanType rp in ratePlans)
                        {
                            //If the name of the subscription rate plan matches that of our initial product rate plan...
                            if(productRatePlan1Name.Equals(rp.RatePlanName))
                            {
                                Console.Out.WriteLine("Previous Rate Plan found: " + rp.ToString());

                                //Store the ID of the existing subscription rate plan
                                existingRpId = rp.Id;
                            }
                        }

                        Console.Out.WriteLine("Calling Subscription Upgrade >");

                        //Upgrade the subscription by passing the Subscription Number, exisitng Subscription Rate Plan, and new Product Rate Plan into our custom method
                        PUTSubscriptionResponseType subUpgradeResponse = manager.upgradeSubscription(accResponse.SubscriptionNumber, existingRpId , productRatePlan2Id);
                        Console.Out.WriteLine("PUTSubscriptionResponseType: " + subUpgradeResponse.ToString());

                        //If there are failure reasons on the subscription upgrade response...
                    
                        //if(subUpgradeResponse.Reasons != null) {
                            //Loop through each failure reason and print it to console for review
                        //    foreach(ErrorCodeType reason in subUpgradeResponse.Reasons)
                        //    {
                        //        Console.Out.WriteLine(reason.ToString());
                        //    }
                        //}                    
                    }
                    else
                    {
                        Console.Out.WriteLine("Account and Subscription creation failed");
                    }
                }
                else
                {
                    Console.Out.WriteLine("Product 2, 'Annual Plan', was not found");
                }
                */

                //If our account and subscription were created successfully...
                if ((accResponse != null) && (accResponse.Success == true))
                {
                    Console.Out.WriteLine("Calling Subscription Cancel >");

                    //★ Delete Subscription
                    /**
                    //Cancel the subscription we created
                    POSTSubscriptionCancellationResponseType cancelResponse = manager.cancelSubscription(accResponse.SubscriptionNumber);
                    Console.Out.WriteLine("POSTSubscriptionCancellationResponsType: " + cancelResponse.ToString());
     　　　　　    */


                    //★★★ Add paymentMethod of Bank Info
                    ProxyActioncreateResponse createPayMethBankRes = manager.createPayMethodBank(accResponse.AccountId);
                    Console.Out.WriteLine("ProxyActioncreateResponse: " + createPayMethBankRes.ToString());


                    //★☆ストップウォッチを開始する。カタログ取得
                    //System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();


                    //★★★ Can not retrieve payment method of Bank Info by 「createPayMethodBank」
                    // Retrieving the Bank Info added by 「createPayMethodBank」                                
                    string selPayMethBankRes = manager.selectPayMethodBank(accResponse.AccountId);
                    Console.Out.WriteLine("ProxyActionqueryResponse: " + selPayMethBankRes.ToString());


                    //★☆ストップウォッチを止める。カタログ
                    //stopwatch.Stop();
                    //★☆結果を表示する。カタログ
                    //Console.WriteLine("SELECT PayMethBank ★ : " + stopwatch.Elapsed);


                    /**
                    //★★★ Add to delete PaymentMethod of CreditCard Info::Unable to delete,cause it is defaultPayment
                    // After adding Banking Info, delete Credit Card Info.
                    // To delete Credit Card Info, change DefaultPaymentID of Account with Bank Info
                    */

                    /**
                    //★ NG1
                    ProxyActionupdateResponse updateDefaultPayMethod 
                        = manager.updatePayMethodId(accResponse.AccountId, selPayMethBankRes);
                    Console.Out.WriteLine("ProxyActionupdateResponse: " + updateDefaultPayMethod.ToString());
                    */

                    /**
                    //★ NG2
                    PUTPaymentMethodResponseType putDefaultPayMethodBank
                        = manager.putPayMethodIdWithBank(selPayMethBankRes);
                    Console.Out.WriteLine("putDefaultPayMethodBank: " + putDefaultPayMethodBank.ToString());
                    */

                    //Account CRUD object/account/
                    ProxyCreateOrModifyResponse accCRUDPayMethodBankUpdate
                        = manager.accountPayMethodBankUpdateCRUD(accResponse.AccountId, selPayMethBankRes);
                    Console.Out.WriteLine("ProxyCreateOrModifyResponse: " + accCRUDPayMethodBankUpdate.ToString());

                    CommonResponseType delPayMethodCardRes = manager.deletePaymentMethod(accResponse.PaymentMethodId);
                    Console.Out.WriteLine("CommonResponseType: " + delPayMethodCardRes.ToString());

                    /** //★★paymentMethodを削除する試みで、AutoPayをfalseにしてDefaultPaymentMethodIDをNULLにしてみる
                    ProxyCreateOrModifyResponse accCRUDPayMethodIdNullUpdate
                        = manager.accountPayMethodIdNullUpdateCRUD(accResponse.AccountId, accResponse.PaymentMethodId);
                      Console.Out.WriteLine("ProxyCreateOrModifyResponse: " + accCRUDPayMethodBankUpdate.ToString());
                    CommonResponseType delPayMethodBankRes = manager.deletePaymentMethod(selPayMethBankRes);
                      Console.Out.WriteLine("CommonResponseType: " + delPayMethodBankRes.ToString());
                    //★★paymentMethodを削除する試みで、AutoPayをfalseにしてDefaultPaymentMethodIDをNULLにしてみる
                    */

                    //★ toDo PaymentMethodStatus = "Closed"; 更新する  bankInfoダミーでもOKなら。。。            
                    //zPayMentBank.PaymentMethodStatus = "Closed";
                    // actionCreateApi.updatePayMethodId（）を流用
                }
                else
                {
                    Console.Out.WriteAsync("Account and subscription were not created and cannot be canceled");
                }

                //Console will await any user input before exiting allowing the user to review the results in the output
                //★ Console.ReadLine();                   

                Console.Out.WriteLine(System.DateTime.Now);
                Console.WriteLine("▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲" + countCsv + "件目");
                countCsv += 1;
            }
            //★ End of READ csv file

            //★ Print Log Out            
            sw.Dispose(); // ファイルを閉じてオブジェクトを破棄  
        }
        
        /** #2 create susscription
         * 
         */
        public void createSubscription()
        {
            try
            { 
                //★ Print Log Out
                // 出力先ファイル名 subscMakeLog   追加書き込み true   文字コードSystem.Text.Encoding.GetEncoding("UTF-8")
                StreamWriter sw = new StreamWriter(subscMakeLog, true, System.Text.Encoding.GetEncoding("UTF-8"));            
                Console.SetOut(sw); // 出力先（Outプロパティ）を設定
                Console.Out.WriteLine("処理開始▶▶▶▶▶▶▶▶▶▶▶▶▶▶▶▶▶▶▶▶" + System.DateTime.Now); //処理開始
                //★ Print Log Out           
            
                //Must set the JSON serializer settings so that null values are not passed to the API
                //Passing null values will result in a failed API call
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                };

                //Initialize the Application Manager which contains all methods and objects used for the QuickStart
                //★ TODO UPDATE WITH YOUR USERNAME AND PASSWORD FOR YOUR ZUORA TEST DRIVE TENANT
                ApplicationManager manager = new ApplicationManager(userID, passWD);

                //★ READ csv file
                //header CSV読込▶
                string csvfileHeader = @subscHeaderCSV;   //CSVファイル格納場所
                TextFieldParser parserHeader = new TextFieldParser(csvfileHeader, Encoding.GetEncoding("UTF-8"));
                parserHeader.TextFieldType = FieldType.Delimited;
                parserHeader.SetDelimiters(","); // 区切り文字はコンマ
                int countCsvHeader = 0;

                //20170901 ★ratePlanDB★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
                // CSV形式のテキストファイルの内容をDataTableに直接格納            
                string fileRP = "file\\2ratePlanDB.csv";   //CSVファイル格納場所（実行ファイル同）
                // データベースへ接続する(今回はCSVファイルを開く)
                OleDbConnection connectionRP = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0; Data Source=" + Path.GetDirectoryName(fileRP) + "\\; Extended Properties=\"Text;HDR=YES;FMT=Delimited\"");
                // クエリ文字列を作る
                // ※ファイルパスを[]でくくること！
                OleDbCommand commandRP = new OleDbCommand("SELECT * FROM [" + Path.GetFileName(fileRP) + "]", connectionRP);
                DataSet datasetRP = new DataSet();
                datasetRP.Clear();    // 念のためクリア
                // CSVファイルの内容をDataSetに入れる
                OleDbDataAdapter adapterRP = new OleDbDataAdapter(commandRP);
                adapterRP.Fill(datasetRP);

                string fileRPC = "file\\2ratePlanChargeDB.csv";
                OleDbConnection connectionRPC = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0; Data Source=" + Path.GetDirectoryName(fileRPC) + "\\; Extended Properties=\"Text;HDR=YES;FMT=Delimited\"");                
                OleDbCommand commandRPC = new OleDbCommand("SELECT * FROM [" + Path.GetFileName(fileRPC) + "]", connectionRPC);
                DataSet datasetRPC = new DataSet();
                datasetRPC.Clear();                
                OleDbDataAdapter adapterRPC = new OleDbDataAdapter(commandRPC);
                adapterRPC.Fill(datasetRPC);

                string fileAccount = "file\\2accountDB.csv";
                OleDbConnection connectionAccount = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0; Data Source=" + Path.GetDirectoryName(fileAccount) + "\\; Extended Properties=\"Text;HDR=YES;FMT=Delimited\"");
                OleDbCommand commandAccount = new OleDbCommand("SELECT * FROM [" + Path.GetFileName(fileAccount) + "]", connectionAccount);
                DataSet datasetAccount = new DataSet();
                datasetAccount.Clear();
                OleDbDataAdapter adapterAccount = new OleDbDataAdapter(commandAccount);
                adapterAccount.Fill(datasetAccount);
                //20170901 ★ratePlanDB★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★

                while (!parserHeader.EndOfData)
                {
                    string[] csvColumnHeader = parserHeader.ReadFields(); // Header 1行読み込み
                    if (countCsvHeader == 0)
                    {
                        countCsvHeader += 1;
                        continue;  //headerは処理しない
                    }

                    Console.WriteLine("▼Header▼" + countCsvHeader + "件目");
                    for (int n = 0; n < csvColumnHeader.Length; n++)
                    {
                        Console.Write(csvColumnHeader[n] + ",");
                    }
                    Console.WriteLine();

                    //detail　CSV読込▶▶▶
                    string csvfile = @subscMakeCSV;   //CSVファイル格納場所
                    TextFieldParser parser = new TextFieldParser(csvfile, Encoding.GetEncoding("UTF-8"));
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(","); // 区切り文字はコンマ                     
                    int countCsv = 0;
                
                    while (!parser.EndOfData)
                    {
                        string[] csvColumn = parser.ReadFields(); // Detail 1行読み込み
                        if (countCsv == 0)
                        {
                            countCsv += 1;
                            continue;  //headerは処理しない
                        }

                        //Header CSVの試算番号　＝　Detail CSVの試算番号をサブスクリプション作成する
                        if (!csvColumnHeader[1].Equals(csvColumn[0]))
                        {
                            continue;
                        }

                        /**  //Header情報表示したので。。。                   
                        // Console.WriteLine("▼▼▼▼▼▼▼▼▼▼" + countCsv + "件目");
                        // for (int n = 0; n < csvColumn.Length; n++)
                        // {
                        // Console.Write(csvColumn[n] + ",");
                        // }
                        // Console.WriteLine();
                        */

                        /** //Name of the initial product rate plan to be added in our new subscription
                        //★ string productRatePlan1Name = "Quarterly Plan";
                        //★★ READ csv file to create Subscription
                        //string productRatePlan1Name = "basicApiPlan";
                        //Name of the "upgraded" product rate plan to be added to the upgraded subscription                
                        //string productRatePlan2Name = "実験プラン";
                        //Creating ojects to store the IDs of the above product rate plans
                        //★ string productRatePlan2Id = null;
                        */
                        string productRatePlan1Id = null;
                        //★ST>親品目コードでサービス分解
                        //string productRatePlan1Name = csvColumn[6]; 
                        string oyaHinmokuCD = csvColumn[3]; //3_品目コード
                        //★☆ストップウォッチを開始する。カタログ取得
                        //System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
                        /** ★product catalog検索は遅い => nameでProductRatePlanを検索
                        Console.Out.WriteLine("Calling Get Catalog >");
                        //Use the GET Catalog call to retrieve the entire product catalog and store it in the catalog container
                        GETCatalogType catalog = manager.catalogApi.GETCatalog();
                        //Get the list of products from the catalog container
                        List<GETProductType> products = catalog.Products;
                        //Loop through all products to find the ones we will use
                        foreach (GETProductType p in products)                {
                            //Get the list of rate plans from the product container
                            List<GETProductRatePlanType> ratePlans = p.ProductRatePlans;
                            //Loop through all rate plans for this product
                            foreach (GETProductRatePlanType rp in ratePlans)                    {
                                //If the initial product rate plan is identified...
                                if (productRatePlan1Name.Equals(rp.Name))                        {
                                    Console.Out.WriteLine("Product Rate Plan 1 found: " + rp.ToString());
                                    //Store the ID for our initial product rate plan
                                    productRatePlan1Id = rp.Id;
                                }                    }                }
                        */
                        //nameでProductRatePlanIDを取得
                        ProxyActionqueryRequest zPtRatePlan = new ProxyActionqueryRequest();
                        //★ST>親品目コードでサービス分解
                        //zPtRatePlan.QueryString = "select id from ProductRatePlan where name = '" + productRatePlan1Name + "'";
                        //productRatePlan1Id =  actionCreateApi.ProxyActionPOSTquery(zPtRatePlan);

                        //★■　親の親品目コードがNULLになったので親は、品目コードで検索する
                        //zPtRatePlan.QueryString = "select id from ProductRatePlan where ParentCode__c = '" + oyaHinmokuCD + "'";
                        zPtRatePlan.QueryString = "select id from ProductRatePlan where ParentCode__c = '" + oyaHinmokuCD + "' or ProductCode__c = '" + oyaHinmokuCD + "'";

                        //■　Configuration.ApiClient.DeserializにてNULLになるので、CSV形式でReturnするよう変更
                        //ProxyActionqueryResponse localVarResponse = actionCreateApi.ProxyActionPOSTqueryOriginal(zPtRatePlan);
                        string ratePlanIdGroup = actionCreateApi.ProxyActionPOSTqueryServiceRateID(zPtRatePlan);
                        //Console.WriteLine("●試算番号：" + csvColumn[0]  + ",親品目：" + oyaHinmokuCD + " 展開するratePlanID : " + ratePlanIdGroup);    //20170823 作成件数確認

                        if (ratePlanIdGroup != null) { 
                            //■　CSV形式でReturnされたratePlanIDを持ってサブスクリプションを作成
                            string[] ServiceRatePlanID = ratePlanIdGroup.Split(',');

                            //20170823 作成件数確認
                            Console.WriteLine("●試算番号：" + csvColumn[0] + ", 親品目：," + oyaHinmokuCD + ", 展開するratePlanID(合計:," + ServiceRatePlanID.Length + ",):," + ratePlanIdGroup);
                            int checkDoneCount = 0;    //20170823 作成件数確認

                            for (int n = 0; n < ServiceRatePlanID.Length; n++)
                            {
                                checkDoneCount = n + 1;

                                productRatePlan1Id = ServiceRatePlanID[n];
                                //★ED>親品目コードでサービス分解
                                /** //★☆ストップウォッチを止める。カタログ
                                //stopwatch.Stop();
                                //★☆結果を表示する。カタログ
                                //Console.WriteLine("Product Rate Plan SELECT★ : " + stopwatch.Elapsed);
                                */
                                //If the initial product rate plan was found...
                                if (productRatePlan1Id != null)
                                {
                                    Console.WriteLine(checkDoneCount + "_サービス展開　：," + productRatePlan1Id);
                                    //★★ READ csv file to create Subscription
                                    //Create an object to store the result of the Create Account call
                                    /** //async
                                    System.Threading.Tasks.Task<POSTSubscriptionResponseType> subscResponse = null;
                                    subscResponse = manager.createSubscrWithCsv(productRatePlan1Id, csvColumn);
                                    Console.Out.WriteLine("POSTSubscriptionResponseType: " + subscResponse.ToString());
                                    Console.Out.WriteLine(System.DateTime.Now);
                                    Console.WriteLine("▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲" + countCsv + "件目");
                                    countCsv += 1;
                                    System.Threading.Thread.Sleep(10); //処理が飛んでたりするので、ちょっとbreak
                                    continue;//次の処理に飛ばす。以下省略
                                    */
                                    //sync                            
                                    /** //★☆ストップウォッチをリセットしてから再開する
                                    //stopwatch.Reset();
                                    //stopwatch.Start();
                                    */
                                    //20170720 status-Pendingのsubscriptionを作成するため 「Action/subscribe」を使用
                                    // 20170721 Cannot deserialize the current JSON array～対応
                                    //POSTSubscriptionResponseType subscResponse = null;
                                    //subscResponse = manager.createSubscrWithCsv(productRatePlan1Id, csvColumn, csvColumnHeader);                            

                                    string subscResponse = null;
                                    //20170901 ★ratePlanDB★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
                                    //subscResponse = manager.actionSubscrWithCsv(productRatePlan1Id, csvColumn, csvColumnHeader);
                                    subscResponse = manager.actionSubscrWithCsv(productRatePlan1Id, csvColumn, csvColumnHeader,datasetRP,datasetRPC,datasetAccount);
                                    /** //★☆ストップウォッチを止める。
                                    //stopwatch.Stop();
                                    //★☆結果を表示する。
                                    //Console.WriteLine("subscription INSERT★★ : " + stopwatch.Elapsed);
                                    */
                                    if (subscResponse != null)    //try catchにてNULLが返された場合のため
                                    {
                                        //20170720 status-Pendingのsubscriptionを作成するため 「Action/subscribe」を使用
                                        //Console.Out.WriteLine("POSTSubscriptionResponseType: " + subscResponse.ToString());
                                        Console.Out.WriteLine("ProxyActionPOSTsubscribe（）: " + subscResponse.ToString());
                                    }
                                    Console.Out.WriteLine(System.DateTime.Now);
                                    continue;//次のCSV処理に
                                }
                                else
                                {
                                    Console.Out.WriteLine("Product Rate Plan 1, 'Quarterly Plan', was not found");
                                }
                            }//■　CSV形式でReturnされたratePlanIDを持ってサブスクリプションを作成

                            //20170823 作成件数確認
                            if (ServiceRatePlanID.Length != checkDoneCount)
                                Console.WriteLine("★★★ error : checkOutサービス展開件数");

                        }
                        else
                        {
                            Console.WriteLine( " 展開するratePlanID がNULL ");
                        }
                        countCsv += 1;
                    }
                    //◀◀◀ End of detail CSV

                    countCsvHeader += 1;
                }
                //◀ End of Header CSV

                //★ Print Log Out            
                sw.Dispose(); // ファイルを閉じてオブジェクトを破棄  
                //★★ READ csv file to create Subscription    
                //System.Threading.Thread.Sleep(1000); //for Async
            }
            catch (Exception ex)
            {                
                Console.WriteLine("createSubscription() failed with message: " + ex.Message);                
            }            
        }//End of createSubscription()

        /** #3 update susscription
        * 
        */
        public void updateSubscription()
        { 
            //★ Print Log Out
            // 出力先ファイル名 "3resultSubscriptUpdate.txt"   追加書き込み true   文字コードSystem.Text.Encoding.GetEncoding("UTF-8")
            StreamWriter sw = new StreamWriter(subscUpdateLog, true, System.Text.Encoding.GetEncoding("UTF-8"));
            Console.SetOut(sw); // 出力先（Outプロパティ）を設定
            Console.Out.WriteLine("処理開始▶▶▶▶▶" + System.DateTime.Now); //処理開始

            //Must set the JSON serializer settings so that null values are not passed to the API
            //Passing null values will result in a failed API call
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            //Initialize the Application Manager which contains all methods and objects used for the QuickStart
            //★ TODO UPDATE WITH YOUR USERNAME AND PASSWORD FOR YOUR ZUORA TEST DRIVE TENANT
            ApplicationManager manager = new ApplicationManager(userID, passWD);

            //★ READ csv file
            string csvfile = subscUpdateCSV;   //CSVファイル格納場所（実行ファイル同）
            TextFieldParser parser = new TextFieldParser(csvfile, Encoding.GetEncoding("UTF-8"));
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(","); // 区切り文字はコンマ

            int countCsv = 0;
            while (!parser.EndOfData)
            {
                string[] csvColumn = parser.ReadFields(); // 1行読み込み

                if (countCsv == 0)
                {
                    countCsv += 1;
                    continue;  //headerは処理しない
                }

                Console.Out.WriteLine(System.DateTime.Now);
                Console.WriteLine("▼ " + countCsv + "件目");

                for (int n = 0; n < csvColumn.Length; n++)
                    Console.Write(csvColumn[n] + ",");

                Console.WriteLine();
                
                // Status＝Activeのみ更新処理を行う：pendingの更新はできん
                PUTSubscriptionResponseType subscResponse = null;
                if (csvColumn[28].Equals("Active"))
                {                    
                    subscResponse = manager.updateSubscrWithCsv(csvColumn);
                    if (subscResponse != null)    //try catchにてNULLが返された場合のため
                        Console.Out.WriteLine("updateSubscrWithCsv(csvColumn) " + subscResponse.ToString());
                }
                else
                {
                    // Action/updateでステータスを更新する(有効日追加)の後、Subscription/updateを実施。
                    // 12_serviceActivationDate,13_customerAcceptanceDateが編集されてるpendingをActiveに更新する
                    if (!string.IsNullOrEmpty(csvColumn[12]) && !string.IsNullOrEmpty(csvColumn[13]))
                    {
                        //Initialize the container　：：： paymentMethodIdの構造体を流用する
                        ProxyActionupdateRequestDefPay zDefPayID = new ProxyActionupdateRequestDefPay();
                        List<ZObjectPayDef> defPayDefIdList = new List<ZObjectPayDef>();
                        ZObjectPayDef zPayDef = new ZObjectPayDef();

                        zPayDef.Id = csvColumn[1];
                        //zPayDef.ServiceActivationDate = DateTime.Parse(csvColumn[12]);
                        //zPayDef.ContractAcceptanceDate = DateTime.Parse(csvColumn[13]);
                        //何で文字列になるんだ～～～
                        //zPayDef.ServiceActivationDate = csvColumn[12];
                        DateTime dateService = DateTime.Parse(csvColumn[12]);
                        string serviceDate = dateService.ToString("yyyy-MM-dd");
                        zPayDef.ServiceActivationDate = serviceDate;
                        //zPayDef.ContractAcceptanceDate = csvColumn[13];
                        DateTime dateContract = DateTime.Parse(csvColumn[13]);
                        string contractDate = dateContract.ToString("yyyy-MM-dd");
                        zPayDef.ContractAcceptanceDate = contractDate;                        

                        defPayDefIdList.Add(zPayDef);

                        zDefPayID.Objects = defPayDefIdList;
                        zDefPayID.Type = "Subscription";
                        
                        ProxyActionupdateResponse updateSubToActive = actionCreateApi.ProxyActionPOSTupdate(zDefPayID);
                        Console.Out.WriteLine("pendingToActive : " + updateSubToActive.ToString());

                        subscResponse = manager.updateSubscrWithCsv(csvColumn);
                        if (subscResponse != null)    //try catchにてNULLが返された場合のため
                            Console.Out.WriteLine("updateSubscrWithCsv(csvColumn)_AfterActivation: " + subscResponse.ToString());
                    }
                    else
                        Console.WriteLine("Statusが「Active」ではないため更新処理できません。");
                }

                countCsv += 1;
                continue;//次のCSV処理に
            }
            //★ End of READ csv file

            //★ Print Log Out            
            sw.Dispose(); // ファイルを閉じてオブジェクトを破棄              
        } //End of updateSubscription()

        /** #4 Make Subscription CSV
        * 
        */
        public void makeSubscriptionCSV()
        {
            StreamWriter sw = null;
            StreamWriter swCsvWrite = null;
            try
            {
                //★ Print Log Out
                // 出力先ファイル名 "4resultMakeSubscriptCSV.txt"   追加書き込み true   文字コードSystem.Text.Encoding.GetEncoding("UTF-8")
                sw = new StreamWriter(subscListLog, true, System.Text.Encoding.GetEncoding("UTF-8"));
                Console.SetOut(sw); // 出力先（Outプロパティ）を設定
                Console.Out.WriteLine("処理開始▶▶▶▶▶" + System.DateTime.Now); //処理開始
                //★ Print Log Out           

                //Must set the JSON serializer settings so that null values are not passed to the API
                //Passing null values will result in a failed API call
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                };

                //Initialize the Application Manager which contains all methods and objects used for the QuickStart
                //★ TODO UPDATE WITH YOUR USERNAME AND PASSWORD FOR YOUR ZUORA TEST DRIVE TENANT
                ApplicationManager manager = new ApplicationManager(userID, passWD);

                //★ READ csv file
                string csvfile = @subscListCSV;   //CSVファイル格納場所（実行ファイル同）
                TextFieldParser parser = new TextFieldParser(csvfile, Encoding.GetEncoding("UTF-8"));
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(","); // 区切り文字はコンマ                     

                // 書込みCSVファイル名：csvForUpdate.csv                
                swCsvWrite = new StreamWriter(@makeSubscCsvList, false, Encoding.GetEncoding("UTF-8"));
                //StreamWriter swCsvWrite = new StreamWriter(@makeSubscCsvList, false, Encoding.GetEncoding("shift_jis"));
            
                while (!parser.EndOfData)
                {
                    string[] subscriptionKey = parser.ReadFields(); // 1行読み込み
                    bool isHeaderWrited = false;    //全体に通して
                    /**  subscription KEY取得 -> ２，０００件の上限あり⇒QueryMore? サブスクリプション作成にてKEYをCSV出力したログを利用？(O)         
                    ProxyActionqueryRequest zSubscKey = new ProxyActionqueryRequest();
                    zSubscKey.QueryString = "select id from subscription";
                    string subscriptionIdGroup = actionCreateApi.ProxyActionPOSTqueryServiceRateID(zSubscKey);
                    //Console.WriteLine(subscriptionIdGroup);
                    //■　CSV形式でReturnされたsubscriptionIdを持ってサブスクリプション情報を作成
                    //string[] subscriptionKey = subscriptionIdGroup.Split(',');
                    */
                    for (int n = 0; n < subscriptionKey.Length; n++)
                    {
                        string subscription1Id = subscriptionKey[n];
                        if (subscription1Id != null)
                        {
                            bool isDetailWrited = false;    //一つのサブスクリプションを通して
                            string chargeDetail = "last-segment";
                            //GETSubscriptionTypeWithSuccess subOne = subscriptionsApi.GETOneSubscription(subscription1Id, chargeDetail);
                            string[] subOne = subscriptionsApi.GETOneSubscription(subscription1Id, chargeDetail);

                            //▼ サブスクリプション情報をCSVに格納
                            // subOne[i] = Header + ￥n + Value　を　HeaderとValueを分けてCSVファイルに書き込む                                                
                            string csvHeader = null;
                            string csvValue = null;

                            for (int i = 0; i < subOne.Length; i++)
                            {
                                //▶ header
                                int foundIndex = subOne[i].IndexOf("\n");
                                if (i == 0)
                                {
                                    csvHeader = subOne[i].Substring(0, foundIndex);
                                }
                                else
                                {
                                    csvHeader += ',';
                                    csvHeader += subOne[i].Substring(0, foundIndex);
                                }
                                //◀ header

                                //▶  Value
                                // Tier(i＝3)はリストになってるので各Tier毎、１行のCSVレコードにする。
                                //"tier,startingUnit,endingUnit,price,priceFormat,ratePlanCharges_Id\n1,1,100,1000,FlatFee,0\n2,101,200,1500,FlatFee,0"
                                if (i == 3) //TierのValue
                                {
                                    string searchWord = "\n";    //検索する文字列
                                    int foundIndexTier = foundIndex;
                                    string strTierAll = null; // 20170720 Tier１行になります。。。(２行だと更新がダメ)
                                    while (0 <= foundIndexTier)
                                    {
                                        string strTier = null;    //Tierの場合、Tierの数分CSVにレコードを追加する。     
                                        //次の検索開始位置
                                        int nextIndexStart = foundIndexTier + searchWord.Length;

                                        //次の位置を探す
                                        int nextFoundIndex = subOne[i].IndexOf(searchWord, nextIndexStart);
                                        if (nextFoundIndex < 0)
                                        {
                                            strTier += ",";
                                            strTier += subOne[i].Substring(foundIndexTier + 1);
                                        }
                                        else
                                        {
                                            strTier += ",";
                                            strTier += subOne[i].Substring(foundIndexTier + 1, nextFoundIndex - foundIndexTier - 1);
                                        }
                                        foundIndexTier = nextFoundIndex;

                                        //Headerは、一回のみ書き込む
                                        if (!isHeaderWrited)
                                        {
                                            swCsvWrite.WriteLine(csvHeader);
                                            isHeaderWrited = true;
                                        }
                                        //Value 書込み 
                                        /** //20170720 Tier１行になります。。。(２行だと更新がダメ)
                                        swCsvWrite.WriteLine(csvValue + strTier);                                        
                                        Console.WriteLine("Value：" + csvValue);
                                        Console.WriteLine("各Tier毎、１行のCSVレコード" + strTier );
                                        isDetailWrited = true;
                                        */
                                        strTierAll += strTier;
                                    } //End Of while (0 <= foundIndexTier)
                                    // 20170720 Tier１行になります。。。(２行だと更新がダメ)
                                    swCsvWrite.WriteLine(csvValue + strTierAll);
                                    Console.WriteLine("Value：" + csvValue);
                                    isDetailWrited = true;

                                } //End of if (i == 3)
                                else  //Tier以外のValue
                                {
                                    if (i == 0)
                                    {
                                        csvValue = subOne[i].Substring(foundIndex + 1);
                                    }
                                    else
                                    {
                                        csvValue += ",";
                                        csvValue += subOne[i].Substring(foundIndex + 1);
                                    }
                                }  //◀ Value
                            }  //for (int i = 0; i < subOne.Length; i++)

                            //Headerは、一回のみ書き込む
                            //if (n == 0)
                            if (!isHeaderWrited)
                            {
                                //TierのHeaderを追加する　by OnCoding 「,tier,startingUnit,endingUnit,price,priceFormat,ratePlanCharges_Id」
                                //if (csvHeader.Length == 1607) //TR sandBox
                                if (csvHeader.Length == 1837)   //MKI sandBox
                                {
                                    swCsvWrite.WriteLine(csvHeader + ",tier,startingUnit,endingUnit,price,priceFormat,ratePlanCharges_Id");
                                }
                                else
                                {
                                    swCsvWrite.WriteLine(csvHeader);
                                }
                                isHeaderWrited = true;
                            }
                            //Value 書込み　Tierにて書込み済みか判断する
                            if (!isDetailWrited)
                            {
                                swCsvWrite.WriteLine(csvValue);
                                Console.WriteLine("Value：" + csvValue);
                            }
                            //▲ サブスクリプション情報をCSVに格納

                            Console.Out.WriteLine(System.DateTime.Now);
                            Console.WriteLine("●　" + (n+1) + "件目");

                            subOne = null; //subOne変数の初期化
                            continue;//
                        }  //if (subscription1Id != null)                       
                    }    //for (int n = 0; n < subscriptionKey.Length; n++)
                }
                sw.Dispose();       // Read ファイルを閉じてオブジェクトを破棄
                swCsvWrite.Close();  // Writeファイルを閉じてオブジェクトを破棄                
            }
            catch (Exception ex)
            {                
                Console.WriteLine("makeSubscriptionCSV() failed with message: " + ex.Message);             
            }

            if (sw != null)  sw.Dispose();            　  // Read ファイルを閉じてオブジェクトを破棄
            if (swCsvWrite != null)  swCsvWrite.Close();  // Writeファイルを閉じてオブジェクトを破棄                
            return;            
        }//End of makeSubscriptionCSV()

        /** #5 delete Subscription
        * 
        */
        public void deleteSubscription()
        {
            //Initialize the Cancel Subscription request container
            POSTSubscriptionCancellationType cancelSub = new POSTSubscriptionCancellationType();

            //Populate the container with all required fields
            cancelSub.CancellationPolicy = "SpecificDate";
            //cancelSub.Invoice = false;
            cancelSub.CancellationEffectiveDate = new DateTime(2017, 07, 01);
            
            string csvfile = @delSub;   //CSVファイル格納場所
            TextFieldParser parser = new TextFieldParser(csvfile, Encoding.GetEncoding("UTF-8"));
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(","); // 区切り文字はコンマ                                 

            while (!parser.EndOfData)
            {
                string[] subNum = parser.ReadFields();

                string subKey = subNum[0];
                Console.Out.WriteLine(" csvRead : " + subKey);

                //POSTSubscriptionCancellationResponseType returnValue = new POSTSubscriptionCancellationResponseType();
                //returnValue = subscriptionsApi.POSTSubscriptionCancellation(subKey, cancelSub, "211.0");
                //string returnValue = subscriptionsApi.POSTSubscriptionCancellationString(subKey, cancelSub, "211.0");
                //Console.Out.WriteLine("POSTSubscriptionCancellationResponsType: " + returnValue.ToString());
                // add 20170921 
                try
                {
                    ProxyDeleteResponse returnValue = new ProxyDeleteResponse();
                    returnValue = subscriptionsApi.ProxyDELETESubscription(subKey);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("deleteSubscription() failed with message: " + ex.Message);
                }
            }
        }


        /** #6 update OyaSubscriptionNo 
        * KMIBS-54 親subScription番号を更新登録する
        */
        public void updateOya()
        {
            //★ Print Log Out
            // 出力先ファイル名 "6oyaAndDate.txt"   追加書き込み true   文字コードSystem.Text.Encoding.GetEncoding("UTF-8")
            StreamWriter sw = new StreamWriter(oyaAndDate , true, System.Text.Encoding.GetEncoding("UTF-8"));
            Console.SetOut(sw); // 出力先（Outプロパティ）を設定
            Console.Out.WriteLine("処理開始▶▶▶▶▶" + System.DateTime.Now); //処理開始

            //Must set the JSON serializer settings so that null values are not passed to the API
            //Passing null values will result in a failed API call
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            //Initialize the Application Manager which contains all methods and objects used for the QuickStart
            //★ TODO UPDATE WITH YOUR USERNAME AND PASSWORD FOR YOUR ZUORA TEST DRIVE TENANT
            ApplicationManager manager = new ApplicationManager(userID, passWD);

            //★ READ csv file
            string csvfile = makeSubscCsvList;   //CSVファイル格納場所
            TextFieldParser parser = new TextFieldParser(csvfile, Encoding.GetEncoding("UTF-8"));
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(","); // 区切り文字はコンマ

            // 20170901 dataSetを毎回読込むのに時間がかかるので、１回のみ読んで引数で渡す
            // CSV形式のテキストファイルの内容をDataTableに直接格納            
            string filename = csvForUpdateDB;   //CSVファイル格納場所（実行ファイル同）
            // データベースへ接続する(今回はCSVファイルを開く)
            OleDbConnection connection = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0; Data Source=" + Path.GetDirectoryName(filename) + "\\; Extended Properties=\"Text;HDR=YES;FMT=Delimited\"");
            // クエリ文字列を作る
            // ※ファイルパスを[]でくくること！
            OleDbCommand command = new OleDbCommand("SELECT * FROM [" + Path.GetFileName(filename) + "]", connection);
            DataSet dataset = new DataSet();
            dataset.Clear();    // 念のためクリア
            // CSVファイルの内容をDataSetに入れる
            OleDbDataAdapter adapter = new OleDbDataAdapter(command);
            adapter.Fill(dataset);            

            int countCsv = 0;
            while (!parser.EndOfData)
            {
                string[] csvColumn = parser.ReadFields(); // 1行読み込み

                if (countCsv == 0)
                {
                    countCsv += 1;
                    continue;  //headerは処理しない
                }

                Console.Out.WriteLine(System.DateTime.Now);
                Console.WriteLine("▼ " + countCsv + "件目");

                for (int n = 0; n < csvColumn.Length; n++)
                    Console.Write(csvColumn[n] + ",");

                Console.WriteLine();
                                
                PUTSubscriptionResponseType subUpResponse = null;

                //子品目の場合、資産番号、親品目コードをキーに親品目のサブスクリプション番号を取得する
                if (!string.IsNullOrEmpty(csvColumn[57]))
                {
                    //20170901 dataSetを毎回読込むのに時間がかかるので、１回のみ読んで引数で渡す
                    //subUpResponse = manager.updateOyaSubscriptionNo(csvColumn);                    
                    subUpResponse = manager.updateOyaSubscriptionNo(csvColumn, dataset);
                    if (subUpResponse != null)    //try catchにてNULLが返された場合のため
                    Console.Out.WriteLine("updateOyaSubscriptionNo() " + subUpResponse.ToString());
                }
                countCsv += 1;
                continue;//次のCSV処理に
            }
            // End of READ csv file
            // Print Log Out            
            sw.Dispose(); // ファイルを閉じてオブジェクトを破棄
            System.Threading.Thread.Sleep(100);
        } //End of updateOya()

        /** #7 品目展開　件数確認
        * 
        */
        public void checksubScriptionCount()
        {
            try
            {
                //★ Print Log Out
                // 出力先ファイル名 subscMakeLog   追加書き込み true   文字コードSystem.Text.Encoding.GetEncoding("UTF-8")
                StreamWriter sw = new StreamWriter("file\\7checksubScriptionCount.csv", true, System.Text.Encoding.GetEncoding("UTF-8"));
                Console.SetOut(sw); // 出力先（Outプロパティ）を設定
                
                //Must set the JSON serializer settings so that null values are not passed to the API
                //Passing null values will result in a failed API call
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                };

                //Initialize the Application Manager which contains all methods and objects used for the QuickStart
                //★ TODO UPDATE WITH YOUR USERNAME AND PASSWORD FOR YOUR ZUORA TEST DRIVE TENANT
                //ApplicationManager manager = new ApplicationManager(userID, passWD);
                
                string csvfileHeader = @"file\\logGrep.csv";   
                TextFieldParser parserHeader = new TextFieldParser(csvfileHeader, Encoding.GetEncoding("UTF-8"));
                parserHeader.TextFieldType = FieldType.Delimited;
                parserHeader.SetDelimiters(","); // 区切り文字はコンマ

                int ind = 1;
                while (!parserHeader.EndOfData)
                {
                    string[] csvColumnHeader = parserHeader.ReadFields(); // Header 1行読み込み                    
                    Console.Out.WriteLine(ind + "," + csvColumnHeader[0] + "," + (csvColumnHeader.Length - 1) );
                    ind += 1;
                }
                //◀ End of Header CSV

                //★ Print Log Out            
                sw.Dispose(); // ファイルを閉じてオブジェクトを破棄  
                //★★ READ csv file to create Subscription    
                //System.Threading.Thread.Sleep(1000); //for Async
            }
            catch (Exception ex)
            {
                Console.WriteLine("checksubScriptionCount() failed with message: " + ex.Message);
            }
        }//End of createSubscription()

    } //End of class ApplicationManager
} // End of namespace
