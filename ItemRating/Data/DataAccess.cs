using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace ItemRating.Data
{
    // one unified location for all the data acces.  Great for breaking up coupling and
    public class DataAccess
    {
        private readonly AppDbContext _db;

        public DataAccess(AppDbContext db)
        {
            _db = db;
        }

        public async Task<ITEM> LoadItemAsync(int? ID_ITEM)
        {
            ITEM temp = new ITEM();
            
            var con = _db.Database.GetDbConnection();

            con.Open();
            using (var command = con.CreateCommand())
            {
                // this does a few things.  First it returns the given item id, the item name, and the current rating aggregate across all ratings for it.
                // additionally it finds the next seqential ID, and the previous seqential ID.  If none found then 0.
                // this is important for the 'next/previous' buttons.
                string q = @" SELECT top(1) b.ID	
		                            ,b.TITLE
		                            ,isnull(C.RATING, 0) as RATING
                              ,(    SELECT isnull(min(a.id), 0)
			                            FROM [test].[dbo].[ITEM] a
			                            where a.id > b.ID) as [NEXT]
                            ,(    SELECT isnull(max(a.id), 0)  
		                            FROM [test].[dbo].[ITEM] a
		                            where a.id < b.ID) as [PREVIOUS]
                              FROM [test].[dbo].[ITEM] b
                              left  JOIN (
	                            SELECT y.[ID]
	                                ,AVG(CAST(z.RATING AS FLOAT)) as RATING
                                FROM [dbo].[ITEM] y
                                JOIN [dbo].[ITEM_RATING] z
		                             ON y.ID = z.ID_ITEM
                                GROUP BY y.ID
                                ) c ON B.ID = c.ID    ";
                if (ID_ITEM != null)
                {
                    // IF there is a specific ID (say from someone clicking 'next' then load that specific item's info, next, previous, name, aggregate rating, etc.
                    q += " WHERE b.ID = @ID_ITEM ";
                    DbParameter tempParameter = command.CreateParameter();
                    tempParameter.ParameterName = "@ID_ITEM";
                    tempParameter.Value = ID_ITEM;
                    command.Parameters.Add(tempParameter);
                }
                command.CommandText = q;

                System.Data.Common.DbDataReader reader = await command.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    reader.Read();
                    temp = new ITEM();
                    temp.ID = (int)reader["ID"];
                    temp.TITLE = (string)reader["TITLE"];
                    temp.RATING = Convert.ToDecimal(reader["RATING"]);
                    temp.NEXT = (int)reader["NEXT"];
                    temp.PREVIOUS = (int)reader["PREVIOUS"];
                }
                reader.Dispose();
            }

            return temp;
        }

        //Get a list of all the ratings for a given ID.
        public async Task<List<ITEM_RATING>> LoadItemsRatingsAsync(int ID_ITEM)
        {
            List<ITEM_RATING> temp = new List<ITEM_RATING>();

            var con = _db.Database.GetDbConnection();
            if (con.State != System.Data.ConnectionState.Open)
                con.Open();

            using (var command = con.CreateCommand())
            {
                string q = @"SELECT [ID_ITEM]
                          ,[ID_USER]
	                      ,b.NAME
                          ,[RATING]
                          ,[DATE_ENTERED]
                      FROM dbo.ITEM_RATING a WITH(NOLOCK)
                      join [USER] b WITH(NOLOCK) ON a.ID_USER = b.ID
                      WHERE [ID_ITEM] = @ID_ITEM 
                    ORDER BY DATE_ENTERED DESC";
                command.CommandText = q;

                DbParameter tempParameter = command.CreateParameter();
                tempParameter.ParameterName = "@ID_ITEM";
                tempParameter.Value = ID_ITEM;
                command.Parameters.Add(tempParameter);

                System.Data.Common.DbDataReader reader = await command.ExecuteReaderAsync();

                temp = new List<ITEM_RATING>();
                if (reader.HasRows)
                {

                    while (reader.Read())
                    {
                        var row = new ITEM_RATING
                        {
                            ID_ITEM = (int)reader["ID_ITEM"],
                            ID_USER = (int)reader["ID_USER"],
                            RATING = (int)(reader["RATING"]),
                            DATE_ENTERED = Convert.ToDateTime(reader["DATE_ENTERED"]),
                            USER_NAME = (string)(reader["NAME"]),
                        };
                        temp.Add(row);
                    }


                }
                reader.Dispose();
            }

            return temp;
        }

        // get a list of all the user's in the table to test adding item ratings from different users.
        public async Task<IList<USER>> LoadUsersAsync()
        {
            List<USER> temp = new List<USER>();

            temp = await _db.USER_DBSet.ToListAsync(); 

            return temp;
        }

        //INSERT or UPATE a given item's rating for a user.
        public async Task UpdateRatingAsync(int ID_ITEM, int ID_USER, int RATING)
        {
            ITEM_RATING temp = await _db.ITEM_RATING_DBSet.FindAsync(ID_ITEM, ID_USER);

            if (temp == null)
            {
                // do insert
                temp = new ITEM_RATING();
                temp.ID_ITEM = ID_ITEM;
                temp.ID_USER = ID_USER;
                temp.RATING = RATING;
                temp.DATE_ENTERED = DateTime.Now;
                _db.ITEM_RATING_DBSet.Add(temp);
                _db.SaveChanges();

            }
            else
            {
                temp.RATING = RATING;
                temp.DATE_ENTERED = DateTime.Now;
                _db.ITEM_RATING_DBSet.Update(temp);
                _db.SaveChanges();
            }
        }

        //Testing Helper - adding items to the table.
        public async Task<int> AddItemAsync(string ItemName)
        {
            ITEM temp = new ITEM()
            {
                TITLE = ItemName
            };

            await _db.ITEM_DBSet.AddAsync(temp);
            await _db.SaveChangesAsync();

            return temp.ID;
        }
    }
}
