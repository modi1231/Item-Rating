# Item-Rating
ASP.NET Core 2 tutorial for an Item Rating system
For: DIC Tutorial on the through process to proof of concept for an item rating system widget.  Core 2 ASP.NET Razor pages.

https://www.dreamincode.net/forums/topic/411210-aspnet-core-2-item-rating-system/

=================
dreamincode.net tutorial backup ahead of decommissioning

 Post icon  Posted 27 May 2018 - 02:01 PM 


Github link: https://github.com/modi1231/Item-Rating

I was in need to build in a rating functionality for a project, and found it would be a great tutorial to highlight some ASP.NET Core 2 functionality.  Ideally this would be something I could moc up, and plunk into a larger project.  Was thinking a more succinct version of Amazon rating.  It seems simple enough, but the complexity starts to show itself in the planning.

There is some moderately complex SQL queries, a bit of Entity Framework, a bit of traditional SQL calls, ASYNC/AWAIT usage, C# functionality to enable/disable buttons in the HTML, and some discussion on coding for testing.

[b][u]Software[/u][/b]
-- Visual Studios 2017
-- MSSQL

[b][u]Concepts[/u][/b]
-- C#
-- Core 2 / Razor pages
-- Entity Framework
-- Database interaction
-- Test Driven Development
-- Asynchronous 
-- Composit EF key

[b][u]Advance Options[/u][/b]
-- comments per user and rating
-- remove rating option
-- javascript stars instead of numbers
-- graphs of ratings per item


[b][u]Decoupling Data Access[/u][/b]
Further down the tutorial you will see where I "decouple" my data access from my code.  This means I start by stubbing out a class, and create my functions away from the code behind in the index.  As a developer you should think about finding "seams" in your code where you can abstract logic.  In this case my index code behind doesn't need to care about where the data comes from, or how it is procured.  Each method only cares that some function returns a specific object or collection of objects.  At the same time the Data Access doesn't need to be concerned about displaying, formatting, etc the data.. just that it gives up a specific object type.  

The Data Access can feed from a file, a database, or even (in the start of it) a hard coded object.  

You would want to do this to make a more loosely coupled set of classes and objects.  This provides a more flexible project, better testing, and as addressed above - better options if you need to swap out data sources as the need changes.  This would be a first toe into the 'test driven development' world.

[b][u]Asynchronous[/u][/b]
By the time this is done you will see quite a bit of AWAIT and ASYNC properties decorating functions and calls.  With Core 2 the default process should be to async most, if not everything, you can.  This helps minimizing blocking of shared worker threads as well as better scalability and performance.  Remember - ASYNC vs SYNC tends to be an all or nothing switch.  Mixing both will, most likely, cause errors and timing problems.

[b][u]The Plan[/u][/b]
The plan is simple - hold "items", "users", and "item ratings" in a database.  Have a way to pull up a given item, and if logged in, assign a rating to it.  Also if an existing rating is there be able to change it.  Ratings should be a per user per item basis.

Ratings would be on the 1-5 scale.

I am thinking textboxes would be for testing and inserting an item name.. drop downs for rating and user name.  A bunch of labels to display item information and who had rated it.

This already gives direction to the code.
1.  A database is needed.
2.  Session variable for 'user log in'.
3.  Save user choices
4.  Load user previous choice.
5.  Testing needs to have ability to:
-- switch users
-- 'log out' users
-- add items
-- navigate the items in the table.

[img]https://i.imgur.com/RfypNlm.jpg[/img]

Buckle in for the setup.  Starting through the database, and winding through the perfunctory needs for this specific app.  So DB layout, model layout, any connection strings, startup edits to the pipeline, and get the ground work done.

You can start with the 'basic' project and remove all the pages but the index (and clear out the code there), or start from a totally empty project and build up.  In this case I went the former.

[b][u]Database[/u][/b]

[u]Item[/u]
Basic incrementing key, and a 'item name'.

[code]
CRE ATE TABLE [dbo].[ITEM](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TITLE] [varchar](20) NULL,
 CONSTRAINT [PK_ITEM] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
[/code]

[u]User[/u]
Like the item - keep it simple.  Incrementing ID and user name.
[code]CRE ATE TABLE [dbo].[USER](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[NAME] [varchar](20) NULL,
 CONSTRAINT [PK_USER] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
[/code]

[u]Item Rating[/u]
A little more complex.  Only one item to one user rating so both item id and user id are they composite key.  The rating value and date entered are kept.
[code]CRE ATE TABLE [dbo].[ITEM_RATING](
	[ID_ITEM] [int] NOT NULL,
	[ID_USER] [int] NOT NULL,
	[RATING] [int] NULL,
	[DATE_ENTERED] [datetime] NULL,
 CONSTRAINT [PK_ITEM_RATING] PRIMARY KEY CLUSTERED 
(
	[ID_ITEM] ASC,
	[ID_USER] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] [/code]

[b][u]Models[/u][/b]
In the code side create a folder 'Data' and three classes for the three tables.  The properites should mimic the matching table information.

It's important to mark 'KEY's here, and provided any data annotations for friendly names.  See the previous Core 2 tutorials that talk to an extent about data annotations.
[code]
    public class ITEM
    {
        [Key]
        public int ID { get; set; }

        [Display(Name = "Title")]
        public string TITLE { get; set; }
    }[/code]

[code]
    public class ITEM_RATING
    {
        [Key]
        public int ID_ITEM { get; set; }

        [Key]
        public int ID_USER { get; set; }

        public int RATING { get; set; }

        public DateTime DATE_ENTERED { get; set; }
    }[/code]

[code]    public class USER
    {
        [Key]
        public int ID { get; set; }

        [Display(Name = "Name")]
        public string NAME { get; set; }
    }[/code]


[b][u]AppDbContext[/u][/b]
At the root, create a AppDbContext.cs class to hold our database context for Entity Framework.

Make sure it inherits DBContext.

Note when specifying a complex composite key, a little bit of tweaking for the 'ITEM RATING' model has to happen.

 [code]    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {

        }
        //Matching dataset to table information.
        public DbSet<ITEM> ITEM_DBSet { get; set; }
        public DbSet<USER> USER_DBSet { get; set; }
        public DbSet<ITEM_RATING> ITEM_RATING_DBSet { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Important to db linking.. Table names _MUST_ be accurate or risk not being found.
            modelBuilder.Entity<ITEM>().ToTable("ITEM");
            modelBuilder.Entity<USER>().ToTable("USER");
            modelBuilder.Entity<ITEM_RATING>().ToTable("ITEM_RATING");

            //composit key
            //https://msdn.microsoft.com/en-us/library/jj591617(v=vs.113).aspx
            modelBuilder.Entity<ITEM_RATING>().HasKey(t => new { t.ID_ITEM, t.ID_USER });

        }
    }[/code] 

[b][u]AppSettings[/u][/b]
Create a 'connection string' item and set your connection string to be used through out the code.  Mine is to my local MSSQL database instance.

If this would go live I would create a more specific user name and password while removing 'integrated security'.

[code]  {
  "Logging": {
    "IncludeScopes": false,
    "LogLevel": {
      "Default"
    }
  },
  "ConnectionStrings": {
    "RatingContext": "Data Source=DESKTOP\\SQLEXPRESS;Initial Catalog=test;Integrated Security=True;Connect Timeout=30"
  }
}[/code]  

[b][u]Startup[/u][/b]
Head to the Startup.cs to add in information for the sessions and services to link dbcontext to the actual database (and DB password).

Here is the AddDbContext and Session information.
[code]         public void ConfigureServices(IServiceCollection services)
        {
            //Important to db linking to actual sql database.
            services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(Configuration.GetConnectionString("RatingContext")));

            services.AddMvc();

            //IMPORTANT to store user id in sesssion.
            services.AddSession(options =>
            {
                // Set a timeout 
                options.IdleTimeout = TimeSpan.FromMinutes(5);
                options.Cookie.HttpOnly = true;
            });

        }[/code]

Make sure to have 'session' info in the pipeline!
[code]        // This method gets called by the run time. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseSession();  // needed for session stuff.  Plus Nuget package.

            app.UseMvc();
        }
[/code]


Wheew..  What a ride.  That should be the last of the big setup.  Get a cookie and let's head to the actual code.

!! An important thing to note is I typically group my 'data access' functionality into a 'data access' class.  This provides a bit of buffering where I know what I want back from the database, but I don't have to hook it up immediately.  I could just return a hard coded object, or array, and continue the work and deal with the nity gritty of the database later.

In the index code behind I always start out with the Dbcontext object and a temporary message string to get basic feedback on actions.

[code]
       private readonly AppDbContext _db;

        [TempData]
        public string Message { get; set; }

Following that it is all the items I will need.  All of these constitute "the model" for index.  Most should be accessible by the HTML, and some will be able to "survive" the return from a post back.

        public ITEM DisplayItem { get; set; } //current active item

        public IEnumerable<SelectListItem> UserDD { get; set; }// DropDown  of users in the database

        [BindProperty]
        public string UserDDSelected { get; set; }//DropDown . gets the selected value on post back.

        public IEnumerable<SelectListItem> ratingValues { get; set; }// DropDown  of 1-5 ratings.

        [BindProperty]
        public ITEM_RATING ITEM_RATINGSelected { get; set; }//DropDown . gets the selected value on post back.

        public IList<ITEM_RATING> RatingsList { get; set; } // saved user ratings for the current item.

        [BindProperty]
        public string NewItemName { get; set; } // for adding items.


Always start out with your index model's constructor.

        public IndexModel(AppDbContext db)
        {
            //Get the database context so we can move data to and from the tables.
            _db = db;
        }
[/code]

The bulk of the fun is on the 'Get' for the page.  I want to take in an item ID (or null) and either load that item's information, or load the first time.  I also want the page to setup the drop down for ratings, pull up any rating for a specific logged in user, and populate the user id drop down.

The "int?" means either an integer or a null.
[code]
   public async Task OnGetAsync(int? ID_ITEM1)
        {
            await LoadItemAsync(ID_ITEM1); // if there is an item load it, else load the first time.

            List<int> temp = new List<int>() { 1, 2, 3, 4, 5 };//for the rating drop downlist

            ratingValues = from a in temp
                           select new SelectListItem
                           {
                               Text = a.ToString(),
                               Value = a.ToString()
                           };

            /// if the user is logged in, grab any rating they may have had for this item.
            if (HttpContext.Session.GetString("id") != null)
            {
                // load user rating and set the ITEM_RATINGSelected.RATING
                if (ID_ITEM1 != null)
                {
                    int ID_USER = Int32.Parse(HttpContext.Session.GetString("id"));
                    ITEM_RATING tempIR = await _db.ITEM_RATING_DBSet.FindAsync(ID_ITEM1, ID_USER);

                    if (tempIR != null)
                    {
                        ITEM_RATINGSelected = tempIR;
                    }
                }
            }

            // in case nothing is found have an instantiated collection.
            if (ITEM_RATINGSelected == null)
                ITEM_RATINGSelected = new ITEM_RATING();

            // the user name list.
            await LoadUsersAsync();
        }
[/code]

The Item load, item rating load, and user id load are less complicated.  I expect, from my data access, to get some sort of item or null back.
[code]
        // load the existing item or the first time.
        private async Task LoadItemAsync(int? ID_ITEM)
        {
            DataAccess temp = new DataAccess(_db);
            DisplayItem = await temp.LoadItemAsync(ID_ITEM);

            if (DisplayItem != null)
                await LoadItemsRatingsAsync(DisplayItem.ID);

        }

        //for a given item id, get any ratings saved in the table.
        private async Task LoadItemsRatingsAsync(int ID_ITEM)
        {
            DataAccess temp = new DataAccess(_db);
            RatingsList = await temp.LoadItemsRatingsAsync(ID_ITEM);
        }

        //load the user list to test log in and rate.
        private async Task LoadUsersAsync()
        {
            DataAccess temp = new DataAccess(_db);
            IList<USER> tempUserList = await temp.LoadUsersAsync();

            if (tempUserList != null && tempUserList.Count > 0)
            {
                UserDD = from a in tempUserList
                         select new SelectListItem
                         {
                             Text = a.NAME,
                             Value = a.ID.ToString()
                         };
            }
            else
            {
                UserDD = new List<SelectListItem>();
            }

            if (HttpContext.Session.GetString("id") != null)
            {
                UserDDSelected = HttpContext.Session.GetString("id");
            }
        }
[/code]

The Update is a bit more tricky.  The HTML action *MUST* be called - in this project - "UpdateRating" for ASP.NET to find the right function from the HTML.  Also the input parameter *MUST* match.

 [code]       //For a given item id, save the selected rating from postback.
        public async Task<IActionResult> OnPostUpdateRatingAsync(int ID_ITEM)
        {
            //Inform the user of much success.
            Message = $"item_id: {ID_ITEM}, value: {ITEM_RATINGSelected.RATING}, user id: {HttpContext.Session.GetString("id")}";

            int ID_USER = Int32.Parse(HttpContext.Session.GetString("id"));

            DataAccess temp = new DataAccess(_db);
            await temp.UpdateRatingAsync(ID_ITEM, ID_USER, ITEM_RATINGSelected.RATING);

            //Send it back to the admin page with the 'current id' to load up.
            return RedirectToPage("/Index", new { ID_ITEM1 = ID_ITEM });
        }[/code]


For my testing I'll have controls on the same page, and I will need to set a user id to session, clear it, or add an item.

   [code]      //testing helper - set user id from drop down list.
        public async Task<IActionResult> OnPostSetUserLogInAsync(int? ID_ITEM)
        {
            //Inform the user of much success.
            Message = $"userDDSelected: {UserDDSelected}";

            if (UserDDSelected != null)
            {
                HttpContext.Session.SetString("id", UserDDSelected);

            }

            //Send it back to the admin page. .. with an item to load and load any given user's ratings.
            return RedirectToPage("/Index", new { ID_ITEM1 = ID_ITEM });

        }

        //testing helper - clear any session information.
        public async Task<IActionResult> OnPostClearLoginAsync()
        {
            //Inform the user of much success.
            Message = $"Clearing";

            HttpContext.Session.Clear();

            //Send it back to the admin page.
            return RedirectToPage("/Index");
        }

        //testing helper - adding item to database
        public async Task<IActionResult> OnPostAddItemAsync()
        {
            DataAccess temp = new DataAccess(_db);
            int newID = await temp.AddItemAsync(NewItemName);

            Message = $"{NewItemName} added";
            //Send it back to the admin page.
            return RedirectToPage("/Index", new { ID_ITEM1 = newID });
        }
 [/code]

Finally the 'Previous' and 'Next' actions need to be setup.  Simply have the main 'Get' load a specific Id.

  [code]       //navigate to the previous item in the db.
        public IActionResult OnPostPreviousItem(int ID_ITEM)
        {
            return RedirectToPage("/Index", new { ID_ITEM1 = ID_ITEM });
        }

        //navigate to the next item in the db.
        public IActionResult OnPostNextItem(int ID_ITEM)
        {
            return RedirectToPage("/Index", new { ID_ITEM1 = ID_ITEM });
        }
 [/code]

My 'DataAccess' class (also in my Data folder) is just stubbed out functions.

  [code]   // one unified location for all the data access.  Great for breaking up coupling and
    public class DataAccess
    {
        private readonly AppDbContext _db;

        public DataAccess(AppDbContext db)
        {
            _db = db;
        }

        public async Task<ITEM> LoadItemAsync(int? ID_ITEM)
        {
            ITEM temp = new ITEM()
            {
                ID = 1,
                TITLE = "foo",
                NEXT = 2,
                PREVIOUS = 0,
                RATING = 5
            };
            return temp;
        }

        //Get a list of all the ratings for a given ID.
        public async Task<List<ITEM_RATING>> LoadItemsRatingsAsync(int ID_ITEM)
        {
            List<ITEM_RATING> temp = new List<ITEM_RATING>();
            temp.Add(new ITEM_RATING()
            {
 ID_ITEM = 1,
  ID_USER = 1,
   RATING = 5,
    USER_NAME = "temp name",
     DATE_ENTERED = DateTime.Now
            });
            return temp;
        }

        // get a list of all the user's in the table to test adding item ratings from different users.
        public async Task<IList<USER>> LoadUsersAsync()
        {
            List<USER> temp = new List<USER>();
            temp.Add(new USER()
            {
                ID = 1,
                NAME = "temp name"
            });
 
            return temp;
        }

        //INSERT or UPATE a given item's rating for a user.
        public async Task UpdateRatingAsync(int ID_ITEM, int ID_USER, int RATING)
        {
        }

        public async Task<int> AddItemAsync(string ItemName)
        {
            return 1;
        }
    }
 [/code]

The HTML is split into two sections.  One for the 'admin' stuff and one for the item interaction.  

I can conditional show/hide information with some C# IF statements.. pretty neat.

   [code]  <div id="left">
        <h3>Admin Testing</h3>   <br />
        <h4>Session state:</h4>
        @*Highlighting conditional code for HTML rendering.*@
        @if (HttpContext.Session.GetString("id") == null)
        {
            <span>Not logged in</span>
        }
        else
        {
            <span>Logged in as id @HttpContext.Session.GetString("id") </span>
        }

        <br />

        Users: @Html.DropDownListFor(a => a.UserDDSelected, Model.UserDD)
@*Notice how the PAGE HANDLER and ROUTe match the code behind function stub?*@
        <button type="submit" asp-page-handler="SetUserLogIn" asp-route-ID_ITEM="@Model.DisplayItem.ID">Log In</button>
         

        <button type="submit" asp-page-handler="ClearLogin">Clear</button>

        <hr />


        <div><h4>Add Item</h4></div>
        Item name: @Html.TextBoxFor(a => a.NewItemName)
        <br />
        <input type="submit" value="Add" asp-page-handler="AddItem" />
        <hr />

    </div> [/code]

The item side has interesting options on disabling buttons if certain values are there, and more specific action handlers and parameters!
 [code]
  <div id="right">
        <h3>Item Testing</h3><br />
        <h4>Navigation</h4> <br />
        @*If no 'previous' id is found then disable the button.. same for 'next'.*@
        <input type="submit" value="Previous" asp-page-handler="PreviousItem" asp-route-ID_ITEM="@Model.DisplayItem.PREVIOUS" disabled="@(Model.DisplayItem.PREVIOUS == 0 ? "disabled" : null)" />
          <input type="submit" value="Next" asp-page-handler="NextItem" asp-route-ID_ITEM="@Model.DisplayItem.NEXT" disabled="@(Model.DisplayItem.NEXT == 0 ? "disabled" : null)" />

        <hr />
        <h4><u>Item</u></h4>
        <div id="item_right">
            <h4>@Model.DisplayItem.TITLE</h4>
            <span>ID: @Model.DisplayItem.ID</span> <br />
            <span>Rating: @Model.DisplayItem.RATING</span><br />

            @Html.DropDownListFor(a => a.ITEM_RATINGSelected.RATING, Model.ratingValues, "--Select a Value--")
            <br />
            <input type="submit" value="Update" asp-page-handler="UpdateRating" asp-route-ID_ITEM="@Model.DisplayItem.ID" disabled="@(HttpContext.Session.GetString("id") == null ? "disabled" : null)" />

            <br />
            <br />
            <span style="font-style:italic;text-decoration:underline;">History:</span><br />
            @*show past ratings from all users for this given item id.*@
            @foreach (var temp in Model.RatingsList)
            {
                <span style="font-style:italic;">@temp.DATE_ENTERED.ToShortDateString() - @temp.USER_NAME - @temp.RATING</span><br />
            }
        </div>
    </div> [/code]

This should be enough to test it all.  I should see my faux data coming in, and the page rendering right.

Time to swing back to the DataAccess and hook up things proper.

The more interesting function is the 'load item'.  The SQL is pretty complex with SELECTS as column information, a left join on a subequery, and conditional WHERE clause.  

You should be able to pull the sections apart and view how they work.  Notice the 'next' and 'previous' rely on the implied concept the item ids are auto increment integers.  If these were, say, random GUIDs I may have to rely on a 'DATE ENTERED' or some other way to logically order the rows in a consistent manner.

   [code]      public async Task<ITEM> LoadItemAsync(int? ID_ITEM)
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
			                            FROM [dbo].[ITEM] a
			                            where a.id > b.ID) as [NEXT]
                            ,(    SELECT isnull(max(a.id), 0)  
		                            FROM [dbo].[ITEM] a
		                            where a.id < b.ID) as [PREVIOUS]
                              FROM [dbo].[ITEM] b
                              left  JOIN (
	                            SEL ECT y.[ID]
	                                ,AVG(CA ST(z.RATING AS FLOAT)) as RATING
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
        } [/code]
You will notice I am storing the information in the 'Item' model, but this has more elements than the table would show.  I added extra columns as 'UNMAPPED' so Entity Framework operations won't get gummed up expecting things when doing quick database hits.  

The model now looks like:

[code]    public class ITEM
    {
        [Key]
        public int ID { get; set; }

        [Display(Name = "Title")]
        public string TITLE { get; set; }

        [NotMapped]
        public decimal RATING { get; set; }

        [NotMapped]
        public Int32 NEXT { get; set; }

        [NotMapped]
        public Int32 PREVIOUS { get; set; }
    }
[/code]

The load item ratings is on par with complexity.  Here I want a list of user names, their ratings, and the date they entered them for a given item id.

  [code]       //Get a list of all the ratings for a given ID.
        public async Task<List<ITEM_RATING>> LoadItemsRatingsAsync(int ID_ITEM)
        {
            List<ITEM_RATING> temp = new List<ITEM_RATING>();

            var con = _db.Database.GetDbConnection();
            if (con.State != System.Data.ConnectionState.Open)
                con.Open();

            using (var command = con.CreateCommand())
            {
                string q = @"SELE CT [ID_ITEM]
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
 [/code]

Once again I am modifying the usual one-to-one mapping of the table columns to my model properties.  In this case 'username' is not a common sight, so I make an unmapped property there too.

[code]    public class ITEM_RATING
    {
        [Key]
        public int ID_ITEM { get; set; }

        [Key]
        public int ID_USER { get; set; }

        public int RATING { get; set; }
        public DateTime DATE_ENTERED { get; set; }

        [NotMapped]
        public string USER_NAME { get; set; }
    }[/code]

Rating update makes a choice on if a value has been entered.. and if so using Entity Framework for the update.

 [code]    //INSERT or UPATE a given item's rating for a user.
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
 [/code]

I hope you enjoyed the tutorial.  Please check the github link for the full project code.

[img]https://i.imgur.com/c7vHdt7.jpg[/img]
