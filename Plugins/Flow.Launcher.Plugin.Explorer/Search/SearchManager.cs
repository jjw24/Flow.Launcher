﻿using Flow.Launcher.Plugin.Explorer.Search.DirectoryInfo;
using Flow.Launcher.Plugin.Explorer.Search.QuickFolderLinks;
using Flow.Launcher.Plugin.Explorer.Search.WindowsIndex;
using Flow.Launcher.Plugin.SharedCommands;
using System;
using System.Collections.Generic;

namespace Flow.Launcher.Plugin.Explorer.Search
{
    public class SearchManager
    {
        private Settings _settings;
        private PluginInitContext _context;

        private IndexSearcher searcher;

        private QuickFolderAccess quickFolderAccess = new QuickFolderAccess();

        public SearchManager(Settings settings, PluginInitContext context)
        {
            _settings = settings;
            _context = context;
            searcher = new IndexSearcher();
        }

        internal List<Result> Search(Query query)
        {
            var querySearch = query.Search;

            var quickFolderLinks = quickFolderAccess.FolderList(query, _settings.FolderLinks);

            if (quickFolderLinks.Count > 0)
                return quickFolderLinks;

            if (EnvironmentVariables.IsEnvironmentVariableSearch(querySearch)) 
            {
                return EnvironmentVariables.GetEnvironmentStringPathSuggestions(querySearch, query);
            }

            // Query is a location path with a full environment variable- starts with a % 
            // and contains another % somewhere before the end of the path
            if (querySearch.Substring(1).Contains("%"))
            {
                querySearch = EnvironmentVariables.TranslateEnvironmentVariablePath(querySearch);
            }

            if (FilesFolders.IsLocationPathString(querySearch))
            {
                return TopLevelFolderSearchBehaviour(WindowsIndexTopLevelFolderSearch,
                                                     DirectoryInfoClassSearch,
                                                     WindowsIndexExists,
                                                     query,
                                                     querySearch);
            }

            return WindowsIndexFilesAndFoldersSearch(query, querySearch);
        }

        private List<Result> DirectoryInfoClassSearch(Query query, string querySearch)
        {
            var directoryInfoSearch = new DirectoryInfoSearch(_settings, _context);

            return directoryInfoSearch.TopLevelDirectorySearch(query, querySearch);
        }

        public List<Result> TopLevelFolderSearchBehaviour(
            Func<Query, string, List<Result>> windowsIndexSearch,
            Func<Query, string, List<Result>> directoryInfoClassSearch,
            Func<string, bool> indexExists,
            Query query,
            string querySearchString)
        {
            var results = windowsIndexSearch(query, querySearchString);

            if (results.Count == 0 && !indexExists(querySearchString))
                return directoryInfoClassSearch(query, querySearchString);

            return results;
        }

        private List<Result> WindowsIndexFilesAndFoldersSearch(Query query, string querySearchString)
        {
            var queryConstructor = new QueryConstructor(_settings);

            return searcher.WindowsIndexSearch(querySearchString,
                                               queryConstructor.CreateQueryHelper().ConnectionString,
                                               queryConstructor.QueryForAllFilesAndFolders,
                                               query);
        }
        
        private List<Result> WindowsIndexTopLevelFolderSearch(Query query, string path)
        {
            var queryConstructor = new QueryConstructor(_settings);

            return searcher.WindowsIndexSearch(path,
                                               queryConstructor.CreateQueryHelper().ConnectionString,
                                               queryConstructor.QueryForTopLevelDirectorySearch,
                                               query);
        }

        private bool WindowsIndexExists(string path)
        {
            return searcher.PathIsIndexed(path);
        }
    }
}