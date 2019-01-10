using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML;
using TheDenOrderingWebApp;
using System.Net.Mail;

namespace TheDenOrderingWebApp
{
    public partial class Order : System.Web.UI.Page
    {
        //bool LoggedIn = false;
        public int numberCode = 0;

        SqlConnection conDeals = new SqlConnection();
        SqlConnection conLogin = new SqlConnection();
        SqlConnection iDenCon = new SqlConnection();
        static string dealsFromData = string.Empty;
     

        protected void Page_Load(object sender, EventArgs e)
        {
            //Database connections
            conLogin.ConnectionString = (@"Data Source=DESKTOP-OGL7383\SQLEXPRESS;Initial Catalog=WebLogin;Integrated Security=True");
            conDeals.ConnectionString = (@"Data Source=DESKTOP-OGL7383\SQLEXPRESS;Initial Catalog=WebDeals;Integrated Security=True");
            iDenCon.ConnectionString = (@"Data Source=DESKTOP-OGL7383\SQLEXPRESS;Initial Catalog=iDen;Integrated Security=True");
           
            //Finds current deal
            conDeals.Open();
            SqlCommand cmdFindDeals = new SqlCommand("SELECT dealText FROM updateDeals", conDeals);
            SqlDataReader rdr = cmdFindDeals.ExecuteReader();
            while (rdr.Read())
            {
                dealsFromData = rdr["dealText"].ToString();
            }
            rdr.Close();
            conDeals.Close();
            //Updates Text
            dealsText.InnerText = dealsFromData;
        }
//LOGIN--------------------------------------------------------------------------------------------------------
        //Login System
        protected void loginClicked(object sender, EventArgs e)
        {
            conLogin.Open();

            string loggedInUser = username.Value;
            string loggedInPass = password.Value;
            //Checks if login values are in table
            SqlCommand cmdLogin = new SqlCommand("SELECT * from Login_Info where Username = @username and Password = @password", conLogin);
            cmdLogin.Parameters.AddWithValue("@username", loggedInUser);
            cmdLogin.Parameters.AddWithValue("@password", loggedInPass);

            SqlDataAdapter da = new SqlDataAdapter(cmdLogin);
            DataTable dt = new DataTable();
            da.Fill(dt);
            //Login values exist
            if (dt.Rows.Count > 0)
            {
                Console.Write("Logged In");
                errorLoginLabel.Visible = false;
                WelcomeLabel.InnerHtml = "Welcome, " + loggedInUser;
                WelcomeLabel.Visible = true;
            }
            //Login values are nonexistent 
            else
            {
                string message = "$('#loginModal').modal({ backdrop: 'static', keyboard: false, show: true });";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", message, true);
                errorLoginLabel.InnerHtml = "Incorrect Username or Password";
                errorLoginLabel.Visible = true;
                WelcomeLabel.Visible = false;
            }

            //username.Value = "";
            //password.Value = "";
            conLogin.Close();
        }
//REGISTER-------------------------------------------------------------------------------------------------------- 
        //Registration System
        protected void registerButton_ServerClick(object sender, EventArgs e)
        {
            string registerName = regName.Value;
            string registerPhone = regPhonenum.Value;
            string registerEmail = regEmail.Value;
            string registerUser = regUser.Value;
            string registerPass = regPass.Value;

            int numLeftBlank = 0;

            //Name is left empty
            if (registerName == string.Empty)
            {
                string message = "$('#registerModal').modal({ backdrop: 'static', keyboard: false, show: true });";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", message, true);
                registerErrorLabel.InnerText = "Please Enter Your Name";
                registerErrorLabel.Visible = true;
                numLeftBlank++;
            }
            //Phonenum is left empty
            if (registerPhone == string.Empty)
            {
                string message = "$('#registerModal').modal({ backdrop: 'static', keyboard: false, show: true });";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", message, true);
                registerErrorLabel.InnerText = "Please Enter Your Phonenumber";
                registerErrorLabel.Visible = true;
                numLeftBlank++;
            }
            //Email is left empty
            if (registerEmail == string.Empty)
            {
                string message = "$('#registerModal').modal({ backdrop: 'static', keyboard: false, show: true });";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", message, true);
                registerErrorLabel.InnerText = "Please Enter Your Email";
                registerErrorLabel.Visible = true;
                numLeftBlank++;
            }
            //Username is left empty
            if (registerUser == string.Empty)
            {
                string message = "$('#registerModal').modal({ backdrop: 'static', keyboard: false, show: true });";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", message, true);
                registerErrorLabel.InnerText = "Please Enter Your Username";
                registerErrorLabel.Visible = true;
                numLeftBlank++;
            }
            //Password is left empty
            if (registerPass == string.Empty)
            {
                string message = "$('#registerModal').modal({ backdrop: 'static', keyboard: false, show: true });";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", message, true);
                registerErrorLabel.InnerText = "Please Enter Your Password";
                registerErrorLabel.Visible = true;
                numLeftBlank++;
            }
            if (numLeftBlank > 1)
            {
                string message = "$('#registerModal').modal({ backdrop: 'static', keyboard: false, show: true });";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", message, true);
                registerErrorLabel.InnerText = "Multiple Items left blank, please enter all information";
                registerErrorLabel.Visible = true;
                numLeftBlank++;
            }
            else
            {
                //Checks if username is already taken
                if (regPass.Value == regConfirmPass.Value)
                {
                    conLogin.Open();
                    SqlCommand cmdExists = new SqlCommand("SELECT COUNT(*) FROM Login_Info where Username=@username", conLogin);
                    cmdExists.Parameters.AddWithValue("@username", registerUser);
                    int usernameCount = (int)cmdExists.ExecuteScalar();
                    if (usernameCount != 0)
                    {
                        string message = "$('#registerModal').modal({ backdrop: 'static', keyboard: false, show: true });";
                        Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", message, true);
                        registerErrorLabel.InnerText = "Username is already used";
                        registerErrorLabel.Visible = true;
                    }
                    else
                    {
                        //Checks if account with email already exists
                        SqlCommand cmdEmailExists = new SqlCommand("SELECT COUNT(*) FROM Login_Info where Email=@email", conLogin);
                        cmdEmailExists.Parameters.AddWithValue("@email", registerEmail);
                        int emailCount = (int)cmdEmailExists.ExecuteScalar();
                        if (emailCount != 0)
                        {
                            string message = "$('#registerModal').modal({ backdrop: 'static', keyboard: false, show: true });";
                            Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", message, true);
                            registerErrorLabel.InnerText = "An Account Is Already Registered With This Email!";
                            registerErrorLabel.Visible = true;
                        }
                        else
                        {
                            SendEmailWithCode();
                        }
                        conLogin.Close();
                    }
                }
                else
                {
                    string message = "$('#registerModal').modal({ backdrop: 'static', keyboard: false, show: true });";
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", message, true);
                    registerErrorLabel.InnerText = "Passwords don't match";
                    registerErrorLabel.Visible = true;
                }
            }
        }
        //Checks Confirmation Number
        protected void btnConfirmation_ServerClick(object sender, EventArgs e)
        {
            string confirmationEntered = confirmationNumInput.Value;

            if (confirmationEntered == WelcomeLabel.InnerText)
            {
                conLogin.Open();
                string registerName = regName.Value;
                string registerPhone = regPhonenum.Value;
                string registerEmail = regEmail.Value;
                string registerUser = regUser.Value;
                string registerPass = regPass.Value;

                SqlCommand cmdInsert = new SqlCommand("insert into Login_Info(Name, Phonenumber, Email, Username, Password)values(@name, @phonenum, @email, @username, @pass)", conLogin);

                cmdInsert.Parameters.AddWithValue("@name", registerName);
                cmdInsert.Parameters.AddWithValue("@phonenum", registerPhone);
                cmdInsert.Parameters.AddWithValue("@email", registerEmail);
                cmdInsert.Parameters.AddWithValue("@username", registerUser);
                cmdInsert.Parameters.AddWithValue("@pass", registerPass);

                int j = cmdInsert.ExecuteNonQuery();
                //Success
                if (j != 0)
                {
                    string message = "$('#youareRegisteredModal').modal({ backdrop: 'static', keyboard: false, show: true });";
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", message, true);
                }
                //Error
                else
                {
                    string message = "$('#errorRegistering').modal({ backdrop: 'static', keyboard: false, show: true });";
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", message, true);
                    errorLabel.InnerText = "Error when registereing";
                }

                conLogin.Close();

                regName.Value = string.Empty;
                regPhonenum.Value = string.Empty;
                regEmail.Value = string.Empty;
                regUser.Value = string.Empty;
                regPass.Value = string.Empty;
            }
            else
            {
                string message = "$('#errorRegistering').modal({ backdrop: 'static', keyboard: false, show: true });";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", message, true);
                errorLabel.InnerHtml = "Incorrect confirmation number" + confirmationEntered + "|" + numberCode;
            }
        }
//ORDER--------------------------------------------------------------------------------------------------------
        //First Submit Button that pops up confirmation
        protected void submitOrder_ServerClick1(object sender, EventArgs e)
        {
            if (WelcomeLabel.Visible == true)
            {
                //Sets string values to order values
                string orderItems = orderItem.Value;
                string orderItemsData = "Order: " + orderItems;
                string orderType = orderingType.Value;
                string orderTypeData = "Type: " + orderType;
                string orderSpecialtyItems = hiddenSpecialty.Value.ToString();
                string orderDetailsPickup = pickUpTime.Value;
                string orderDetailsPickupData = "Pickup Time: " + orderDetailsPickup;
                string orderDetailsDelivery = deliveryLocation.Value;
                string orderDetailsDeliveryData = "Delivery Location: " + orderDetailsDelivery;
                string orderTotal = hiddenTotal.Value.ToString();

                string message = "$('#confirmationPopup').modal({ backdrop: 'static', keyboard: false, show: true });";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", message, true);

                //Confirmation Text
                if (orderType == "Delivery")
                {
                    confirmOrderBody.InnerText = "Please confirm the order of a Delivery at the " + orderDetailsDelivery + ". Order: " + orderItems + " + " + orderSpecialtyItems + " | " + "total: " + orderTotal;
                }
                if (orderType == "Pick-up")
                {
                    confirmOrderBody.InnerText = "Please confirm the order of a Pickup at " + orderDetailsPickup + ". Order: " + orderItems + " + " + orderSpecialtyItems + " | " + "total: " + orderTotal; ;
                }
            }
            else
            {
                string message = "$('#notLoggedInError').modal({ backdrop: 'static', keyboard: false, show: true });";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", message, true);
            }
        }

        //Proccesses user order and pushes it to the database
        protected void submitOrder_ServerClick(object sender, EventArgs e)
        {
            string orderItems = orderItem.Value;
            string orderItemsData = "Order: " + orderItems;
            string orderType = orderingType.Value;
            string orderTypeData = "Type: " + orderType;
            string orderSpecialtyItems = hiddenSpecialty.Value.ToString();
            string orderSpecialtyData = "Specialty: " + orderSpecialtyItems;
            string orderDetailsPickup = pickUpTime.Value;
            string orderDetailsPickupData = "Pickup Time: " + orderDetailsPickup;
            string orderDetailsDelivery = deliveryLocation.Value;
            string orderDetailsDeliveryData = "Delivery Location: " + orderDetailsDelivery;
            string orderTotal = hiddenTotal.Value.ToString();
            string orderTotalData = "Total: " + orderTotal;
            string phoneNum = string.Empty;
            string name = string.Empty;

            string loggedInUser = username.Value;
            string loggedInPass = password.Value;

            conLogin.Open();
            SqlCommand cmdFindPhone = new SqlCommand("SELECT Phonenumber from Login_Info WHERE (Username=@username)", conLogin);
            cmdFindPhone.Parameters.AddWithValue("@username", loggedInUser);
            //Find Phonenumber
            SqlDataReader read = cmdFindPhone.ExecuteReader();
            while (read.Read())
            {
                phoneNum = (read["Phonenumber"].ToString());
            }
            read.Close();
            //Find Name
            SqlCommand cmdFindName = new SqlCommand("SELECT Name from Login_Info WHERE (Username=@username)", conLogin);
            cmdFindName.Parameters.AddWithValue("@username", loggedInUser);

            SqlDataReader readName = cmdFindName.ExecuteReader();
            while (readName.Read())
            {
                name = (readName["Name"].ToString());
            }
            readName.Close();

            string phoneNumber = phoneNum;
            string customerNameData = "Name: " + name;

            SqlCommand cmd = new SqlCommand("insert into orderUC(OrderItems, OrderType, OrderDetails, OrderSpecialty, OrderTotal, UserPhone, CustomerName)values(@orderItem, @orderType, @orderDetails, @orderSpecialty, @orderTotal, @userPhoneNum, @customerName)", iDenCon);
            iDenCon.Open();
            if (orderType == "Delivery")
            {
                cmd.Parameters.AddWithValue("@orderItem", orderItemsData);
                cmd.Parameters.AddWithValue("@orderType", orderTypeData);
                cmd.Parameters.AddWithValue("@orderDetails", orderDetailsDeliveryData);
                cmd.Parameters.AddWithValue("@orderSpecialty", orderSpecialtyData);
                cmd.Parameters.AddWithValue("@orderTotal", orderTotalData);
                cmd.Parameters.AddWithValue("@userPhoneNum", phoneNumber);
                cmd.Parameters.AddWithValue("@customerName", customerNameData);
                cmd.ExecuteNonQuery();
            }

            else if (orderType == "Pick-up")
            {
                cmd.Parameters.AddWithValue("@orderItem", orderItemsData);
                cmd.Parameters.AddWithValue("@orderType", orderTypeData);
                cmd.Parameters.AddWithValue("@orderDetails", orderDetailsPickupData);
                cmd.Parameters.AddWithValue("@orderSpecialty", orderSpecialtyData);
                cmd.Parameters.AddWithValue("@orderTotal", orderTotalData);
                cmd.Parameters.AddWithValue("@userPhoneNum", phoneNumber);
                cmd.Parameters.AddWithValue("@customerName", customerNameData);
                cmd.ExecuteNonQuery();
            }

            //Sets All values to empty
            orderItem.Value = string.Empty;
            orderingType.Value = string.Empty;
            pickUpTime.Value = string.Empty;
            deliveryLocation.Value = string.Empty;

            conLogin.Close();
            iDenCon.Close();
        }
        
        //Resets Login Table 
        private void TRUNCATElogin()
        {
            conLogin.Open();

            SqlCommand truncateLogin = new SqlCommand("TRUNCATE TABLE Login_Info", conLogin);
            truncateLogin.ExecuteNonQuery();

            conLogin.Close();
        }
        //Sends Conformation Code
        private void SendEmailWithCode()
        {
            int _max = 9999;
            int _min = 1000;
            Random rdm = new Random();
            numberCode = rdm.Next(_min, _max);
            WelcomeLabel.Visible = false;
            WelcomeLabel.InnerText = numberCode.ToString();

            string userEmail = regEmail.Value;

            //From, To
            MailMessage mail = new MailMessage("blhsTheDen@gmail.com", userEmail);
            SmtpClient client = new SmtpClient("smtp.gmail.com");
            client.Port = 587;
            //From Login
            client.Credentials = new System.Net.NetworkCredential("blhsTheDen@gmail.com", "usd45817");
            mail.Subject = "Confirmation Code";
            mail.Body = "Here is your confirmation code: " + numberCode;
            client.EnableSsl = true;
            //Sends Mail    
            client.Send(mail);

            string message = "$('#confirmEmail').modal({ backdrop: 'static', keyboard: false, show: true });";
            Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", message, true);
        }
        //Forgot login modal
        protected void btnForgotLogin_ServerClick(object sender, EventArgs e)
        {
            string message = "$('#forgotLoginModal').modal({ backdrop: 'static', keyboard: false, show: true });";
            Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", message, true);
        }
 //FORGOT LOGIN--------------------------------------------------------------------------------------------------------
        //Forgot login system
        private void sendForgotLoginInfo()
        {
            string email = forgotLoginEmail.Value;

            string username = string.Empty;
            string password = string.Empty;
             
            //Checks if account with email exists
                SqlCommand cmdFindInfo = new SqlCommand("SELECT Username, Password FROM Login_Info WHERE(Email=@email)", conLogin);
                cmdFindInfo.Parameters.AddWithValue("@email", email);
                SqlDataReader rdr = cmdFindInfo.ExecuteReader();
                while (rdr.Read())
                {
                    username = rdr["Username"].ToString();
                    password = rdr["Password"].ToString();
                }
                rdr.Close();
                conLogin.Close();

                MailMessage mail = new MailMessage("blhsTheDen@gmail.com", email);
                SmtpClient client = new SmtpClient("smtp.gmail.com");
                client.Port = 587;
                //From Login
                client.Credentials = new System.Net.NetworkCredential("blhsTheDen@gmail.com", "usd45817");
                mail.Subject = "Login Info";
                mail.Body = "Here is you login info! " + Environment.NewLine + "Username: " + username + Environment.NewLine + "Password: " + password;
                client.EnableSsl = true;
                //Sends Mail    
                client.Send(mail);
        }
        //Sends login info
        protected void btnSendEmailForgot_ServerClick(object sender, EventArgs e)
        {
            string userEmail = forgotLoginEmail.Value;

            if(userEmail != string.Empty)
            {
                conLogin.Open();
                SqlCommand cmdEmailNotFound = new SqlCommand("SELECT COUNT(*) FROM Login_Info where Email=@email", conLogin);
                cmdEmailNotFound.Parameters.AddWithValue("@email", userEmail);
                int emailCount = (int)cmdEmailNotFound.ExecuteScalar();
                //If email does not exist
                if (emailCount != 0)
                {
                    string message = "$('#forgotLoginModal').modal({ backdrop: 'static', keyboard: false, show: true });";
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", message, true);
                    errorForgotLogin.InnerText = "email does exist!";
                    errorForgotLogin.Visible = true;
                    conLogin.Close();
                }
                //Email exists
                else
                {
                    sendForgotLoginInfo();
                    string message = "$('#forgotLoginModal').modal({ backdrop: 'static', keyboard: false, show: true });";
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", message, true);
                    errorForgotLogin.InnerText = "Email with login informaiton was sent!";
                    errorForgotLogin.Visible = true;
                }    
            }
            else
            {
                string message = "$('#forgotLoginModal').modal({ backdrop: 'static', keyboard: false, show: true });";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", message, true);
                errorForgotLogin.InnerText = "email not entered";
                errorForgotLogin.Visible = true;
            }
        }
    }
}
