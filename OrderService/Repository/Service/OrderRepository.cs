using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OrderService.Repository.Interface;
using System.Data;
using System.Transactions;

namespace OrderService.Repository.Service
{
    public class OrderRepository : IOrderRepository
    {
        private IConfiguration Configuration;
        private SqlConnection con;
        public OrderRepository(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }

        private void connection()
        {
            string constr = this.Configuration.GetConnectionString("ConnStringDb");
            con = new SqlConnection(constr);
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
        }

        public async Task<List<OrderModel>> GetOrder(string UserName)
        {
            List<OrderModel> EmpList = new List<OrderModel>();
            try
            {
                connection();
                SqlCommand cmd = new SqlCommand("sp_GetOrder", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserName", UserName);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    OrderModel order = new OrderModel
                    {
                        selectedTable = dt.Rows[0]["TableNo"].ToString(),
                        orderItems = new List<OrderModel.OrderItem>()
                    };
                    foreach (DataRow dr in dt.Rows)
                    {
                        OrderModel.OrderItem item = new OrderModel.OrderItem
                        {
                            name = dr["Name"].ToString(),
                            full = Convert.ToInt32(dr["FullPortion"]),
                            half = Convert.ToInt32(dr["HalfPortion"])
                        };
                        order.orderItems.Add(item);
                    }
                    EmpList.Add(order);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return EmpList;
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
            return EmpList;
        }

        public async Task<bool> AddOrder(OrderModel order)
        {
            bool flag = false;
            connection();
            try
            {
                using (SqlTransaction transaction = con.BeginTransaction())
                {
                    SqlCommand cmd = new SqlCommand("sp_InsertOrder", con, transaction);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TableNo", Convert.ToInt32(order.selectedTable));
                    cmd.Parameters.AddWithValue("@CreatedBy", order.userName);

                    var orderItemsTable = new DataTable();
                    orderItemsTable.Columns.Add("Name", typeof(string));
                    orderItemsTable.Columns.Add("FullPortion", typeof(int));
                    orderItemsTable.Columns.Add("HalfPortion", typeof(int));

                    foreach (var item in order.orderItems)
                    {
                        orderItemsTable.Rows.Add(item.name, item.full, item.half);
                    }
                    cmd.Parameters.AddWithValue("@OrderItems", orderItemsTable);

                    int i = await cmd.ExecuteNonQueryAsync();

                    if (i == order.orderItems.Count)
                    {
                        transaction.Commit();
                        flag = true;
                    }
                    else
                    {
                        transaction.Rollback();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return flag;
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
            return flag;
        }

        public async Task<Tuple<bool, string>> IsAuthenticated(string username, string password)
        {
            bool isSuccess = false;
            string token = String.Empty;
            try
            {
                connection();
                SqlCommand cmd = new SqlCommand("sp_IsAuthenticated", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Password", password);
                SqlParameter userExistFlag = new SqlParameter("@userExistFlag", SqlDbType.Bit);
                userExistFlag.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(userExistFlag);
                SqlParameter Token = new SqlParameter("@Token", SqlDbType.VarChar, 3000);
                Token.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(Token);
                await cmd.ExecuteNonQueryAsync();
                isSuccess = Convert.ToBoolean(userExistFlag.Value);
                token = Convert.ToString(Token.Value);
                if (Convert.ToBoolean(isSuccess))
                {
                    return Tuple.Create(isSuccess, string.IsNullOrEmpty(token) ? "" : token);
                }
                else
                {

                    return Tuple.Create(false, "");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return Tuple.Create(isSuccess, string.IsNullOrEmpty(token) ? "" : token);
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }

        public async Task<bool> InsertToken(string username, string token, DateTime expiryDate)
        {
            bool flag = false;
            try
            {
                connection();
                SqlCommand cmd = new SqlCommand("sp_InsertToken", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Token", token);
                cmd.Parameters.AddWithValue("@ExpiryDate", expiryDate);
                int i = await cmd.ExecuteNonQueryAsync();
                if (i > 0)
                {
                    flag = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return flag;
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
            return flag;
        }
    }
}
