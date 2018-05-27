//-x- item table
//-- -- id
//-- -- name

//-x- user table
//-- -- id
//-- -- name

//--x item rating table
//-- -- item id
//-- -- user id
//-- -- rating
//-- -- date

//"log in"
//-x- drop down
//-x- button

//clear login
//-x- button

//-x- load item

//-x- if logged in see if rating..
//-x- if so make that hte drop down
//-x- if not then default 'select rating'
//-x- save button disabled if not logged in.

//-x- item load
//-x- -- aggregate rating
//-x- show who voted
//-- remove rating?
//-- comments
//-x- change item
//-x- -- next
//-x- -- previous
//-x- add item
//-x- data access class
//-x- clean up
//-x- async it all?

//-- write up

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ItemRating.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ItemRating.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _db;

        [TempData]
        public string Message { get; set; }

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

        public IndexModel(AppDbContext db)
        {
            //Get the database context so we can move data to and from the tables.
            _db = db;
        }

        public async Task OnGetAsync(int? id_item)
        {
            await LoadItemAsync(id_item); // if there is an item load it, else load the first time.

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
                if (id_item != null)
                {
                    int ID_USER = Int32.Parse(HttpContext.Session.GetString("id"));
                    ITEM_RATING tempIR = await _db.ITEM_RATING_DBSet.FindAsync(id_item, ID_USER);

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

        //For a given item id, save the selected rating from postback.
        public async Task<IActionResult> OnPostUpdateRatingAsync(int ID_ITEM)
        {
            //Inform the user of much success.
            Message = $"item_id: {ID_ITEM}, value: {ITEM_RATINGSelected.RATING}, user id: {HttpContext.Session.GetString("id")}";

            int ID_USER = Int32.Parse(HttpContext.Session.GetString("id"));

            DataAccess temp = new DataAccess(_db);
            await temp.UpdateRatingAsync(ID_ITEM, ID_USER, ITEM_RATINGSelected.RATING);

            //Send it back to the admin page with the 'current id' to load up.
            return RedirectToPage("/Index", new { id_item = ID_ITEM });
        }

        //testing helper - set user id from drop down list.
        public async Task<IActionResult> OnPostSetUserLogInAsync(int? ID_ITEM)
        {
            //Inform the user of much success.
            Message = $"userDDSelected: {UserDDSelected}";

            if (UserDDSelected != null)
            {
                HttpContext.Session.SetString("id", UserDDSelected);

            }

            //Send it back to the admin page. .. with an item to load and load any given user's ratings.
            return RedirectToPage("/Index", new { id_item = ID_ITEM });

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
            return RedirectToPage("/Index", new { id_item = newID });
        }

        //navigate to the previous item in the db.
        public IActionResult OnPostPreviousItem(int ID_ITEM)
        {
            return RedirectToPage("/Index", new { id_item = ID_ITEM });
        }

        //navigate to the next item in the db.
        public IActionResult OnPostNextItem(int ID_ITEM)
        {
            return RedirectToPage("/Index", new { id_item = ID_ITEM });
        }

    }
}
