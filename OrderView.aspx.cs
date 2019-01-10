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
using System.Net.Mail;


namespace TheDenOrderingWebApp
{
    public partial class OrderView : System.Web.UI.Page
    {
        SqlConnection iDenCon = new SqlConnection();
        SqlConnection conLogin = new SqlConnection();
        
        public int numOrdersTotal = 0;
        static int numBobcatOrdered = 0;
        public double bobcatTotal = 0;
        static int numCoffeeOrdered = 0;
        public double coffeeTotal = 0;
        static int numLemonadeOrdered = 0;
        public double lemonadeTotal = 0;
        static int numHotChocOrdered = 0;
        public double hotchocTotal = 0;
        static int numChaiOrdered = 0;
        public double chaiTotal = 0;

        public double totalMade = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            iDenCon.ConnectionString = (@"Data Source=DESKTOP-OGL7383\SQLEXPRESS;Initial Catalog=iDen;Integrated Security=True");
            conLogin.ConnectionString = (@"Data Source=DESKTOP-OGL7383\SQLEXPRESS;Initial Catalog=WebLogin;Integrated Security=True");

        }
//LOGIN---------------------------------------------------------------------------------------------------------
        protected void loginBtn_ServerClick(object sender, EventArgs e)
        {
            string emailEntered = enteredEmail.Value;
            string lowerEmailEntered = emailEntered.ToLower();

            string passwordEntered = enteredPass.Value;
            string lowerPasswordEntered = passwordEntered.ToLower();

            if(lowerEmailEntered == "blhstheden@gmail.com" || lowerPasswordEntered == "usd45817")
            {
                loginModal.Visible = false;
                buttons.Visible = true;
            }
            else
            {
                errorLogin.InnerText = "Incorrect email or password";
                errorLogin.Visible = true;
            }
        }
//UNCOMPLETE ORDERS---------------------------------------------------------------------------------------------------------
        //Shows orders that are not started
        protected void unCompleteOrdersbtn_ServerClick(object sender, EventArgs e)
        {
            unCompleteOrders.Visible = true;
            workingOnOrders.Visible = false;
            outboundOrders.Visible = false;

            using (iDenCon)
            {
                iDenCon.Open();
                SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM orderUC", iDenCon);
                DataTable dtbl = new DataTable();
                sqlDa.Fill(dtbl);
                unCompleteOrdersTable.DataSource = dtbl;
                unCompleteOrdersTable.DataBind();
                iDenCon.Close();
            }
        }
 //WORKING ON---------------------------------------------------------------------------------------------------------
        //Shows orders that are being worked-on
        protected void workOnOrdersbtn_ServerClick(object sender, EventArgs e)
        {
            unCompleteOrders.Visible = false;
            workingOnOrders.Visible = true;
            outboundOrders.Visible = false;

            using (iDenCon)
            {
                iDenCon.Open();
                SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM ordersWO", iDenCon);
                DataTable dtbl = new DataTable();
                sqlDa.Fill(dtbl);
                workingOnTable.DataSource = dtbl;
                workingOnTable.DataBind();
                iDenCon.Close();
            }

        }
//OUTBOUND ---------------------------------------------------------------------------------------------------------
        //Shows orders that are outbound to the location of the delivery
        protected void outBoundOrdersbtn_ServerClick(object sender, EventArgs e)
        {
            unCompleteOrders.Visible = false;
            workingOnOrders.Visible = false;
            outboundOrders.Visible = true;

            using (iDenCon)
            {
                iDenCon.Open();
                SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM orderOB", iDenCon);
                DataTable dtbl = new DataTable();
                sqlDa.Fill(dtbl);
                outboundTable.DataSource = dtbl;
                outboundTable.DataBind();
                iDenCon.Close();
            }
        }
//UNCOMPLETE ---> WORKING ON---------------------------------------------------------------------------------------------------------
        //Sends order from uncomplete to working on
        protected void btnSendToWO_ServerClick(object sender, EventArgs e)
        {
            string idEntered = inputUCtoWO.Value;

            string OrderidData = string.Empty;
            string orderItemsData = string.Empty;
            string orderTypeData = string.Empty;
            string orderDetailsData = string.Empty;
            string orderSpecialtyData = string.Empty;
            string orderTotalData = string.Empty;
            string userPhoneData = string.Empty;
            string customerNameData = string.Empty;

            SqlDataReader rdr = null;
            SqlCommand cmd = null;
            iDenCon.Open();
            string commandText = "SELECT OrderID, OrderItems, OrderType, OrderDetails, OrderSpecialty, OrderTotal, UserPhone, CustomerName FROM orderUC WHERE(OrderID='" + idEntered + "')";
            cmd = new SqlCommand(commandText);
            cmd.Connection = iDenCon;
            rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                OrderidData = rdr["OrderID"].ToString();
                orderItemsData = rdr["OrderItems"].ToString();
                orderTypeData = rdr["OrderType"].ToString();
                orderDetailsData = rdr["OrderDetails"].ToString();
                orderSpecialtyData = rdr["OrderSpecialty"].ToString();
                orderTotalData = rdr["OrderTotal"].ToString();
                userPhoneData = rdr["UserPhone"].ToString();
                customerNameData = rdr["CustomerName"].ToString();
            }
            rdr.Close();
            iDenCon.Close();

            iDenCon.Open();
            //Inserts into working on table
            SqlCommand cmdInsertWO = new SqlCommand("insert into ordersWO(OrderID, OrderItems, OrderType, OrderDetails, OrderSpecialty, OrderTotal, UserPhone, CustomerName)values(@id, @orderItem, @orderType, @orderDetails, @orderSpecialty, @orderTotal, @userPhoneNum, @customerName)", iDenCon);
            cmdInsertWO.Parameters.AddWithValue("@id", OrderidData);
            cmdInsertWO.Parameters.AddWithValue("@orderItem", orderItemsData);
            cmdInsertWO.Parameters.AddWithValue("@orderType", orderTypeData);
            cmdInsertWO.Parameters.AddWithValue("@orderDetails", orderDetailsData);
            cmdInsertWO.Parameters.AddWithValue("@orderSpecialty", orderSpecialtyData);
            cmdInsertWO.Parameters.AddWithValue("@orderTotal", orderTotalData);
            cmdInsertWO.Parameters.AddWithValue("@userPhoneNum", userPhoneData);
            cmdInsertWO.Parameters.AddWithValue("@customerName", customerNameData);
            int x = cmdInsertWO.ExecuteNonQuery();
            if (x != 0)
            {
                //Deletes from UC table
                SqlCommand cmdDelete = null;
                string commandDelText = "DELETE from orderUC WHERE (OrderID='" + idEntered + "')";
                cmdDelete = new SqlCommand(commandDelText);
                cmdDelete.Connection = iDenCon;
                cmdDelete.ExecuteNonQuery();
                iDenCon.Close();

                string message = "$('#moveSuccessful').modal({ backdrop: 'static', keyboard: false, show: true });";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", message, true);
            }
            else
            {
                string message = "$('#moveError').modal({ backdrop: 'static', keyboard: false, show: true });";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", message, true);
            }
        }

        protected void btnSendToWObefore_ServerClick(object sender, EventArgs e)
        {
            string idEntered = inputUCtoWO.Value;

            string message = "$('#confirmMoveUCtoWO').modal({ backdrop: 'static', keyboard: false, show: true });";
            Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", message, true);
            bodyconfirmMoveUCtoWO.InnerText = "Confirm moving order id:" + idEntered + " to Working On!";
            bodyconfirmMoveUCtoWO.Visible = true;
        }
        //Deal of the day
        protected void dealOfTheDaybtn_ServerClick(object sender, EventArgs e)
        {

        }

        //Confirm WO to OB
        protected void btnSendToOBbefore_ServerClick(object sender, EventArgs e)
        {
            string idEntered = inputWOtoOB.Value;

            string message = "$('#confirmWOtoOB').modal({ backdrop: 'static', keyboard: false, show: true });";
            Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", message, true);
            bodyconfirmWOtoOB.InnerText = "Confirm moving order id:" + idEntered + " to Outbound orders!";
            bodyconfirmWOtoOB.Visible = true;
        }
//WORKING ON ---> OUTBOUND ---------------------------------------------------------------------------------------------------------
        //Moves order from WO to OB
        protected void btnSendToOB_ServerClick(object sender, EventArgs e)
        {
            string idEntered = inputWOtoOB.Value;

            string OrderidData = string.Empty;
            string orderItemsData = string.Empty;
            string orderTypeData = string.Empty;
            string orderDetailsData = string.Empty;
            string orderSpecialtyData = string.Empty;
            string orderTotalData = string.Empty;
            string userPhoneData = string.Empty;
            string customerNameData = string.Empty;

            SqlDataReader rdr = null;
            SqlCommand cmd = null;
            iDenCon.Open();
            string commandText = "SELECT OrderID, OrderItems, OrderType, OrderDetails, OrderSpecialty, OrderTotal, UserPhone, CustomerName FROM ordersWO WHERE(OrderID='" + idEntered + "')";
            cmd = new SqlCommand(commandText);
            cmd.Connection = iDenCon;
            rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                OrderidData = rdr["OrderID"].ToString();
                orderItemsData = rdr["OrderItems"].ToString();
                orderTypeData = rdr["OrderType"].ToString();
                orderDetailsData = rdr["OrderDetails"].ToString();
                orderSpecialtyData = rdr["OrderSpecialty"].ToString();
                orderTotalData = rdr["OrderTotal"].ToString();
                userPhoneData = rdr["UserPhone"].ToString();
                customerNameData = rdr["CustomerName"].ToString();
            }
            rdr.Close();
            iDenCon.Close();

            iDenCon.Open();
            //Inserts into OB table
            SqlCommand cmdInsertOB = new SqlCommand("insert into orderOB(OrderID, OrderItems, OrderType, OrderDetails, OrderSpecialty, OrderTotal, UserPhone, CustomerName)values(@id, @orderItem, @orderType, @orderDetails, @orderSpecialty, @orderTotal, @userPhoneNum, @customerName)", iDenCon);
            cmdInsertOB.Parameters.AddWithValue("@id", OrderidData);
            cmdInsertOB.Parameters.AddWithValue("@orderItem", orderItemsData);
            cmdInsertOB.Parameters.AddWithValue("@orderType", orderTypeData);
            cmdInsertOB.Parameters.AddWithValue("@orderDetails", orderDetailsData);
            cmdInsertOB.Parameters.AddWithValue("@orderSpecialty", orderSpecialtyData);
            cmdInsertOB.Parameters.AddWithValue("@orderTotal", orderTotalData);
            cmdInsertOB.Parameters.AddWithValue("@userPhoneNum", userPhoneData);
            cmdInsertOB.Parameters.AddWithValue("@customerName", customerNameData);
            int x = cmdInsertOB.ExecuteNonQuery();
            iDenCon.Close();
            if (x != 0)
            {        
                string message = "$(#moveSuccessful').modal({ backdrop: 'static', keyboard: false, show: true });";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", message, true);

                //string phonenumber = string.Empty;
                string emailUser = string.Empty;
                string name = string.Empty;

                iDenCon.Open();
                ////Finds Phonenumber value
                SqlCommand cmd1 = new SqlCommand("SELECT CustomerName FROM orderOB WHERE (OrderID='" + idEntered + "')", iDenCon);
                SqlDataReader reader = null;
                reader = cmd1.ExecuteReader();
                while (reader.Read())
                {
                    name = (reader["CustomerName"].ToString());
                }
                reader.Close();
                iDenCon.Close();

                conLogin.Open();
                SqlCommand cmd2 = new SqlCommand("SELECT Email FROM Login_Info WHERE (Name=@name)", conLogin);
                cmd2.Parameters.AddWithValue("@name", name);
                SqlDataReader reader2 = null;
                reader2 = cmd2.ExecuteReader();
                while (reader2.Read())
                {
                    emailUser = (reader2["Email"].ToString());
                }
                reader2.Close();
                conLogin.Close();

                string emailRetrieved = emailUser;

                //From, To
                MailMessage mail = new MailMessage("blhsTheDen@gmail.com", "alexwbailey7@gmail.com");
                SmtpClient client = new SmtpClient("smtp.gmail.com");
                client.Port = 587;
                //From Login
                client.Credentials = new System.Net.NetworkCredential("blhsTheDen@gmail.com", "usd45817");
                mail.Subject = "The Den Order Complete";
                mail.Body = "Your order is complete!";
                client.EnableSsl = true;
                //Sends Mail    
                client.Send(mail);

                //Deletes from WO table
                iDenCon.Open();
                SqlCommand cmdDelete = null;
                string commandDelText = "DELETE from ordersWO WHERE (OrderID='" + idEntered + "')";
                cmdDelete = new SqlCommand(commandDelText);
                cmdDelete.Connection = iDenCon;
                cmdDelete.ExecuteNonQuery();
                iDenCon.Close();
            }
            else
            {
                string message = "$('#moveError').modal({ backdrop: 'static', keyboard: false, show: true });";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", message, true);
            }
        }

        protected void btnOrderDoneBefore_ServerClick(object sender, EventArgs e)
        {
            string idEntered = inputDone.Value;

            string message = "$('#confirmOBtoDel').modal({ backdrop: 'static', keyboard: false, show: true });";
            Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", message, true);
            bodyconfirmOBtoDel.InnerText = "Confirm that order id:" + idEntered + " is done!";
            bodyconfirmOBtoDel.Visible = true;
        }

        protected void btnOrderDone_ServerClick(object sender, EventArgs e)
        {
            string idEntered = inputDone.Value;
            string itemOrdered = string.Empty;

            iDenCon.Open();
            //Finds Phonenumber value
            SqlCommand cmd1 = new SqlCommand("SELECT OrderItems FROM orderOB WHERE (OrderID='" + idEntered + "')", iDenCon);
            SqlDataReader reader = null;
            reader = cmd1.ExecuteReader();
            while (reader.Read())
            {
                itemOrdered = reader["OrderItems"].ToString();
            }
            reader.Close();

            if (itemOrdered.Contains("Bobcat"))
            {
                numBobcatOrdered++;
            }
            else if (itemOrdered.Contains("Coffee"))
            {
                numCoffeeOrdered++;
            }
            else if (itemOrdered.Contains("Lemonade"))
            {
                numLemonadeOrdered++;
            }
            else if (itemOrdered.Contains("Hot Chocolate"))
            {
                numHotChocOrdered++;
            }
            else if (itemOrdered.Contains("Chai"))
            {
                numChaiOrdered++;
            }

            //Send Text : Thank you text
            //Deletes from OB table
            SqlCommand cmdDelete = null;
            string commandDelText = "DELETE from orderOB WHERE (OrderID='" + idEntered + "')";
            cmdDelete = new SqlCommand(commandDelText);
            cmdDelete.Connection = iDenCon;
            cmdDelete.ExecuteNonQuery();
            iDenCon.Close();

            string message = "$('#moveSuccessful').modal({ backdrop: 'static', keyboard: false, show: true });";
            Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", message, true);
        }

        private void TRUNCATEuc()
        {
            iDenCon.Open();
            SqlCommand cmd = new SqlCommand("TRUNCATE TABLE orderUC", iDenCon);
            cmd.ExecuteNonQuery();
            iDenCon.Close();
        }
        private void TRUNCATEwo()
        {
            iDenCon.Open();
            SqlCommand cmd = new SqlCommand("TRUNCATE TABLE ordersWO", iDenCon);
            cmd.ExecuteNonQuery();
            iDenCon.Close();
        }
        private void TRUNCATEob()
        {
            iDenCon.Open();
            SqlCommand cmd = new SqlCommand("TRUNCATE TABLE OrdersOB_Info", iDenCon);
            cmd.ExecuteNonQuery();
            iDenCon.Close();
        }

        protected void btnStartDay_ServerClick(object sender, EventArgs e)
        {
            btnStartDay.Visible = false;
            btnEndDay.Visible = true;

            orderBTNs.Visible = true;
        }
//END OF DAY---------------------------------------------------------------------------------------------------------
        protected void btnEndDay_ServerClick(object sender, EventArgs e)
        {   
            btnStartDay.Visible = true;
            btnEndDay.Visible = false;

            orderBTNs.Visible = false;
            outboundOrders.Visible = false;

            //Send to this email
            string email = "blhsTheDen@gmail.com";

            //Finds Total
            bobcatTotal = 2.50 * numBobcatOrdered;
            coffeeTotal = 2.00 * numCoffeeOrdered;
            lemonadeTotal = 1.00 * numLemonadeOrdered;
            hotchocTotal = 1.00 * numHotChocOrdered;
            chaiTotal = 1.50 * numChaiOrdered;

            string date = DateTime.Now.ToString("MM/dd");

            totalMade = bobcatTotal + coffeeTotal + lemonadeTotal + hotchocTotal + chaiTotal;

            //Email Body
            string mailBody = "Here is today's online report!" + Environment.NewLine + Environment.NewLine +
                "Number of Bobcats Sold: " + numBobcatOrdered + Environment.NewLine + Environment.NewLine +
                "Number of Coffees Sold: " + numCoffeeOrdered + Environment.NewLine + Environment.NewLine +
                "Number of Lemonades Sold: " + numLemonadeOrdered + Environment.NewLine + Environment.NewLine +
                "Number of Hot Chocolate Sold: " + numHotChocOrdered + Environment.NewLine + Environment.NewLine +
                "Number of Chais Sold: " + numChaiOrdered + Environment.NewLine + Environment.NewLine +
                " --- TOTAL: $" + totalMade + " ---" + Environment.NewLine + Environment.NewLine;

            //From, To
            MailMessage mail = new MailMessage(email, email);
            SmtpClient client = new SmtpClient("smtp.gmail.com");
            client.Port = 587;
            //From Login
            client.Credentials = new System.Net.NetworkCredential("blhsTheDen@gmail.com", "usd45817");
            mail.Subject = date + " - Today's Report";
            mail.Body = mailBody;

            client.EnableSsl = true;
            //Sends Mail   
            client.Send(mail);

            string message = "$('#emailSent').modal({ backdrop: 'static', keyboard: false, show: true });";
            Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", message, true);
        }
    }
}
