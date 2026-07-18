using System.Collections.Generic;
using UnityEngine;

namespace Juul
{
    public class SearchManager
    {
        public static bool IsSearching = false;
        public static bool WasSearchingLastFrame = false;
        public static string SearchQuery = "";

        public static void PerformSearch()
        {
            Category searchCat = new Category();
            searchCat.Name = "Search Results";
            searchCat.Buttons = new List<Button>();
            searchCat.Subcategories = new List<Category>();

            string query = SearchQuery.ToLower();
            if (!string.IsNullOrEmpty(query) && Buttons.Modules != null)
            {
                foreach (Category module in Buttons.Modules)
                {
                    if (module == ExtraButtons.EnabledCategory) continue;
                    if (module == PlayerMenu.GetPlayersCategory()) continue;
                    SearchInCategory(module, query, searchCat);
                }
            }
            Core.ActiveCategory = searchCat;
            Core.CurrentPage = 0;
        }

        private static void SearchInCategory(Category category, string query, Category results)
        {
            if (category.Buttons != null)
            {
                foreach (Button b in category.Buttons)
                {
                    if (b.Name.ToLower().Contains(query))
                    {
                        results.Buttons.Add(b);
                    }
                }
            }

            if (category.Subcategories != null)
            {
                foreach (Category subcat in category.Subcategories)
                {
                    SearchInCategory(subcat, query, results);
                }
            }
        }
    }
}
