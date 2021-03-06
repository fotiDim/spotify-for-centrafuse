﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpotiFire.SpotifyLib;
using centrafuse.Plugins;
using System.Threading;
using System.Windows.Forms;
using System.Data;
using System.Drawing;

namespace Spotify
{
    public partial class Spotify
    {
        private void LoadInboxTracks()
        {
            if (CheckLoggedInAndOnline())
            {
                CF_systemCommand(CF_Actions.SHOWINFO, "Retrieving your inbox");
                ThreadPool.QueueUserWorkItem(delegate(object obj)
                {
                    IEnumerable<ITrack> inboxTracks = null;
                    try
                    {
                        using (var inboxList = SpotifySession.Inbox)
                        {
                            SleepUntilTrue(() => inboxList.IsLoaded);

                            inboxTracks = inboxList.Tracks.Cast<ITrack>().ToArray();
                        }
                    }
                    catch (Exception ex)
                    {
                        this.ParentForm.BeginInvoke(new MethodInvoker(() =>
                        {
                            CF_systemCommand(CF_Actions.HIDEINFO);
                            WriteError(ex);
                            CF_displayMessage(ex.Message);
                        }));
                        return;
                    }
                    
                    var table = LoadTracksIntoTable(inboxTracks);

                    this.ParentForm.BeginInvoke(new MethodInvoker(delegate()
                    {
                        CF_systemCommand(CF_Actions.HIDEINFO);
                        SwitchToTab(Tabs.Inbox, GroupingType.Songs, table, "Inbox Songs", null,  true);
                    }));
                });
            }
        }

        private void LoadInboxPlaylists()
        {
            if (CheckLoggedInAndOnline())
            {
                CF_systemCommand(CF_Actions.SHOWINFO, "Retrieving your inbox");
                ThreadPool.QueueUserWorkItem(delegate(object obj)
                {
                    try
                    {
                        IEnumerable<ITrack> inboxTracks = null;
                        try
                        {
                            using (var inboxList = SpotifySession.Inbox)
                            {
                                SleepUntilTrue(() => inboxList.IsLoaded);

                                inboxTracks = inboxList.Tracks.Cast<ITrack>().ToArray();
                            }
                        }
                        catch (Exception ex)
                        {
                            this.ParentForm.BeginInvoke(new MethodInvoker(delegate()
                            {
                                CF_systemCommand(CF_Actions.HIDEINFO);
                                CF_displayMessage(ex.Message);
                                WriteError(ex);
                            }));
                            return;
                        }

                        var placeholderLinks = inboxTracks.Where(t => t.IsPlaceholder).Select(t => t.CreateLink()).ToArray();
                        var playlists = placeholderLinks.Where(l => l.Type == sp_linktype.SP_LINKTYPE_PLAYLIST).Select(l => l.As<IPlaylist>()).ToArray();

                        foreach (var link in placeholderLinks)
                            link.Dispose();

                        SleepUntilTrue(() => playlists.All(p => p.IsLoaded));

                        var table = LoadPlaylistsIntoTable(playlists);

                        this.ParentForm.BeginInvoke(new MethodInvoker(delegate()
                        {
                            CF_systemCommand(CF_Actions.HIDEINFO);
                            SwitchToTab(Tabs.Inbox, GroupingType.Playlists, table, "Inbox Playlists", null, true);
                        }));
                    }
                    catch (Exception ex)
                    {
                        this.ParentForm.BeginInvoke(new MethodInvoker(() =>
                        {
                            CF_systemCommand(CF_Actions.HIDEINFO);
                            WriteError(ex);
                            CF_displayMessage(ex.Message);
                        }));
                    }
                });
                
            }
        }

        private void LoadInboxAlbums()
        {
            if (CheckLoggedInAndOnline())
            {
                CF_systemCommand(CF_Actions.SHOWINFO, "Retrieving your inbox");
                ThreadPool.QueueUserWorkItem(delegate(object obj)
                {
                    try
                    {
                        IEnumerable<ITrack> inboxTracks = null;
                        try
                        {
                            using (var inboxList = SpotifySession.Inbox)
                            {
                                SleepUntilTrue(() => inboxList.IsLoaded);

                                inboxTracks = inboxList.Tracks.Cast<ITrack>().ToArray();
                            }
                        }
                        catch (Exception ex)
                        {
                            this.ParentForm.BeginInvoke(new MethodInvoker(delegate()
                            {
                                CF_systemCommand(CF_Actions.HIDEINFO);
                                CF_displayMessage(ex.Message);
                                WriteError(ex);
                            }));
                            return;
                        }

                        var placeholderLinks = inboxTracks.Where(t => t.IsPlaceholder).Select(t => t.CreateLink()).ToArray();
                        var albumLinks = placeholderLinks.Where(l => l.Type == sp_linktype.SP_LINKTYPE_ALBUM);
                        var albums = placeholderLinks.Where(l => l.Type == sp_linktype.SP_LINKTYPE_ALBUM).Select(l => l.As<IAlbum>()).ToArray();

                        foreach (var link in placeholderLinks)
                            link.Dispose();

                        SleepUntilTrue(() => albums.All(a => a.IsLoaded));

                        albums = albums.Where(a =>
                            {
                                bool success = true;
                                try
                                {
                                    var artist = a.Artist;
                                }
                                catch
                                {
                                    success = false;
                                }

                                return success;
                            }).ToArray();

                        var table = LoadAlbumsIntoTable(albums, true);

                        this.ParentForm.BeginInvoke(new MethodInvoker(delegate()
                        {
                            CF_systemCommand(CF_Actions.HIDEINFO);
                            SwitchToTab(Tabs.Inbox, GroupingType.Albums, table, "Inbox Albums", null, true);
                        }));
                    }
                    catch (Exception ex)
                    {
                        this.ParentForm.BeginInvoke(new MethodInvoker(() =>
                        {
                            CF_systemCommand(CF_Actions.HIDEINFO);
                            WriteError(ex);
                            CF_displayMessage(ex.Message);
                        }));
                    }
                });
            }
        }

        private void LoadStarredTracks()
        {
            if (CheckLoggedInAndOnline())
            {
                CF_systemCommand(CF_Actions.SHOWINFO, "Retrieving your starred songs");
                ThreadPool.QueueUserWorkItem(delegate(object obj)
                {
                    try
                    {
                        IEnumerable<ITrack> starredTracks = null;
                        try
                        {
                            using (var starredList = SpotifySession.Starred)
                            {
                                SleepUntilTrue(() => starredList.IsLoaded);

                                starredTracks = starredList.Tracks.Cast<ITrack>();
                            }
                        }
                        catch (Exception ex)
                        {
                            this.ParentForm.BeginInvoke(new MethodInvoker(delegate()
                            {
                                CF_systemCommand(CF_Actions.HIDEINFO);
                                CF_displayMessage(ex.Message);
                                WriteError(ex);
                            }));
                            return;
                        }

                        var table = LoadTracksIntoTable(starredTracks);

                        this.ParentForm.BeginInvoke(new MethodInvoker(delegate()
                        {
                            CF_systemCommand(CF_Actions.HIDEINFO);
                            SwitchToTab(Tabs.Popular, GroupingType.Songs, table, "Starred tracks", null, true);
                        }));
                    }
                    catch (Exception ex)
                    {
                        this.ParentForm.BeginInvoke(new MethodInvoker(() =>
                        {
                            CF_systemCommand(CF_Actions.HIDEINFO);
                            WriteError(ex);
                            CF_displayMessage(ex.Message);
                        }));
                    }
                });
            }
        }

        private void LoadPlaylists()
        {
            if (CheckLoggedIn())
            {
                CF_systemCommand(CF_Actions.SHOWINFO, "Retrieving your playlists");
                ThreadPool.QueueUserWorkItem(delegate(object obj)
                {
                    try
                    {
                        IEnumerable<IPlaylist> playlists = null;
                        try
                        {
                            var container = SpotifySession.PlaylistContainer;

                            SleepUntilTrue(() => container.IsLoaded);

                            playlists = container.Playlists.Cast<IPlaylist>();
                        }
                        catch (Exception ex)
                        {
                            this.ParentForm.BeginInvoke(new MethodInvoker(delegate()
                            {
                                CF_systemCommand(CF_Actions.HIDEINFO);
                                CF_displayMessage(ex.Message);
                                WriteError(ex);
                            }));
                            return;
                        }

                        var table = LoadPlaylistsIntoTable(playlists);

                        this.ParentForm.BeginInvoke(new MethodInvoker(delegate()
                        {
                            CF_systemCommand(CF_Actions.HIDEINFO);
                            SwitchToTab(Tabs.Playlists, GroupingType.Playlists, table, "Playlists", null, true);
                        }));
                    }
                    catch (Exception ex)
                    {
                        this.ParentForm.BeginInvoke(new MethodInvoker(() =>
                        {
                            CF_systemCommand(CF_Actions.HIDEINFO);
                            WriteError(ex);
                            CF_displayMessage(ex.Message);
                        }));
                    }
                });
            }
        }

        private void LoadNowPlaying()
        {
            int? scrollState = null;
            if(currentTrack != null)
            {
                var currentRow = this.NowPlayingTable.AsEnumerable().Single(row => (row["TrackObject"] as ITrack) == currentTrack);
                scrollState = NowPlayingTable.Rows.IndexOf(currentRow);
            }

            SwitchToTab(Tabs.NowPlaying, GroupingType.Songs, NowPlayingTable, "Now Playing", null, true, scrollState);
        }

        private const int TopListLimit = 200;
        private void LoadPopularTracks()
        {
            if (CheckLoggedInAndOnline())
            {
                CF_systemCommand(CF_Actions.SHOWINFO, "Retrieving Top List");
                ThreadPool.QueueUserWorkItem(delegate(object obj)
                {
                    try
                    {
                        IEnumerable<ITrack> popularTracks = null;
                        try
                        {
                            popularTracks = SpotifySession.GetTopList(ToplistType.Tracks, TopListLimit).Cast<ITrack>();
                        }
                        catch (Exception ex)
                        {
                            this.ParentForm.BeginInvoke(new MethodInvoker(delegate()
                            {
                                CF_systemCommand(CF_Actions.HIDEINFO);
                                CF_displayMessage(ex.Message);
                                WriteError(ex);
                            }));
                            return;
                        }

                        var table = LoadTracksIntoTable(popularTracks);

                        this.ParentForm.BeginInvoke(new MethodInvoker(delegate()
                        {
                            CF_systemCommand(CF_Actions.HIDEINFO);
                            SwitchToTab(Tabs.Popular, GroupingType.Songs, table, "Popular tracks", null, true);
                        }));
                    }
                    catch (Exception ex)
                    {
                        this.ParentForm.BeginInvoke(new MethodInvoker(() =>
                        {
                            CF_systemCommand(CF_Actions.HIDEINFO);
                            WriteError(ex);
                            CF_displayMessage(ex.Message);
                        }));
                    }
                });
            }
        }

        private void LoadPopularAlbums()
        {
            if (CheckLoggedInAndOnline())
            {
                CF_systemCommand(CF_Actions.SHOWINFO, "Retrieving Top List");
                ThreadPool.QueueUserWorkItem(delegate(object obj)
                {
                    try
                    {
                        IEnumerable<IAlbum> popularAlbums = null;
                        try
                        {
                            popularAlbums = SpotifySession.GetTopList(ToplistType.Albums, TopListLimit).Cast<IAlbum>();
                        }
                        catch (Exception ex)
                        {
                            this.ParentForm.BeginInvoke(new MethodInvoker(delegate()
                            {
                                CF_systemCommand(CF_Actions.HIDEINFO);
                                CF_displayMessage(ex.Message);
                                WriteError(ex);
                            }));
                            return;
                        }

                        var table = LoadAlbumsIntoTable(popularAlbums, false);

                        this.ParentForm.BeginInvoke(new MethodInvoker(delegate()
                        {
                            CF_systemCommand(CF_Actions.HIDEINFO);
                            SwitchToTab(Tabs.Popular, GroupingType.Albums, table, "Popular albums", null, true);
                        }));
                    }
                    catch (Exception ex)
                    {
                        this.ParentForm.BeginInvoke(new MethodInvoker(() =>
                        {
                            CF_systemCommand(CF_Actions.HIDEINFO);
                            WriteError(ex);
                            CF_displayMessage(ex.Message);
                        }));
                    }
                });
            }
        }

        private void LoadPopularArtists()
        {
            if (CheckLoggedInAndOnline())
            {
                CF_systemCommand(CF_Actions.SHOWINFO, "Retrieving Top List");
                ThreadPool.QueueUserWorkItem(delegate(object obj)
                {
                    try
                    {
                        IEnumerable<IArtist> popularArtists = null;
                        try
                        {
                            popularArtists = SpotifySession.GetTopList(ToplistType.Artists, TopListLimit).Cast<IArtist>();
                        }
                        catch (Exception ex)
                        {
                            this.ParentForm.BeginInvoke(new MethodInvoker(delegate()
                            {
                                CF_systemCommand(CF_Actions.HIDEINFO);
                                CF_displayMessage(ex.Message);
                                WriteError(ex);
                            }));
                            return;
                        }

                        var table = LoadArtistsIntoTable(popularArtists);

                        this.ParentForm.BeginInvoke(new MethodInvoker(delegate()
                        {
                            CF_systemCommand(CF_Actions.HIDEINFO);
                            SwitchToTab(Tabs.Popular, GroupingType.Artists, table, "Popular artists", null, true);
                        }));
                    }
                    catch (Exception ex)
                    {
                        this.ParentForm.BeginInvoke(new MethodInvoker(() =>
                        {
                            CF_systemCommand(CF_Actions.HIDEINFO);
                            WriteError(ex);
                            CF_displayMessage(ex.Message);
                        }));
                    }
                });
            }
        }

        private void LoadTrackSearch()
        {
            if (CheckLoggedInAndOnline())
            {
                CFDialogParams searchDialogParams = new CFDialogParams("Search by song name");
                CFDialogResults results = new CFDialogResults();
                DialogResult result = CF_displayDialog(CF_Dialogs.OSK, searchDialogParams, results);
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    string searchText = results.resultvalue;
                    CF_systemCommand(CF_Actions.SHOWINFO, "Searching...");
                    ThreadPool.QueueUserWorkItem(delegate(object param)
                    {
                        try
                        {
                            var search = SpotifySession.SearchTracks(searchText, 0, 20);

                            SleepUntilTrue(() => search.IsComplete);

                            List<ITrack> tracks = new List<ITrack>();
                            foreach (var track in search.Tracks)
                            {
                                if (track.IsAvailable)
                                {
                                    tracks.Add(track);
                                }
                            }
                            search.Dispose();
                            var table = LoadTracksIntoTable(tracks);

                            this.ParentForm.BeginInvoke(new MethodInvoker(delegate()
                            {
                                CF_systemCommand(CF_Actions.HIDEINFO);
                                SwitchToTab(Tabs.Search, GroupingType.Songs, table, "Search", null, true);
                            }));
                        }
                        catch (Exception ex)
                        {
                            this.ParentForm.BeginInvoke(new MethodInvoker(() =>
                            {
                                CF_systemCommand(CF_Actions.HIDEINFO);
                                WriteError(ex);
                                CF_displayMessage(ex.Message);
                            }));
                        }
                    });
                }
            }
        }

        private void LoadAlbumSearch()
        {
            if (CheckLoggedInAndOnline())
            {
                CFDialogParams searchDialogParams = new CFDialogParams("Search by album name");
                CFDialogResults results = new CFDialogResults();
                DialogResult result = CF_displayDialog(CF_Dialogs.OSK, searchDialogParams, results);
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    string searchText = results.resultvalue;
                    CF_systemCommand(CF_Actions.SHOWINFO, "Searching...");
                    ThreadPool.QueueUserWorkItem(delegate(object param)
                    {
                        try
                        {
                            var search = SpotifySession.SearchAlbums(searchText, 0, 20);

                            SleepUntilTrue(() => search.IsComplete);

                            List<IAlbum> albums = new List<IAlbum>();
                            foreach (var album in search.Albums)
                            {
                                if (album.IsAvailable)
                                {
                                    albums.Add(album);
                                }
                            }
                            search.Dispose();
                            var table = LoadAlbumsIntoTable(albums, false);

                            this.ParentForm.BeginInvoke(new MethodInvoker(delegate()
                            {
                                CF_systemCommand(CF_Actions.HIDEINFO);
                                SwitchToTab(Tabs.Search, GroupingType.Albums, table, "Search", null, true);
                            }));
                        }
                        catch (Exception ex)
                        {
                            this.ParentForm.BeginInvoke(new MethodInvoker(() =>
                            {
                                CF_systemCommand(CF_Actions.HIDEINFO);
                                WriteError(ex);
                                CF_displayMessage(ex.Message);
                            }));
                        }
                    });
                }
            }
        }

        private void LoadArtistSearch()
        {
            if (CheckLoggedInAndOnline())
            {
                CFDialogParams searchDialogParams = new CFDialogParams("Search by artist name");
                CFDialogResults results = new CFDialogResults();
                DialogResult result = CF_displayDialog(CF_Dialogs.OSK, searchDialogParams, results);
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    string searchText = results.resultvalue;
                    CF_systemCommand(CF_Actions.SHOWINFO, "Searching...");
                    ThreadPool.QueueUserWorkItem(delegate(object param)
                    {
                        try
                        {
                            var search = SpotifySession.SearchArtists(searchText, 0, 20);

                            SleepUntilTrue(() => search.IsComplete);

                            List<IArtist> artists = new List<IArtist>();
                            foreach (var artist in search.Artists)
                            {
                                artists.Add(artist);
                            }
                            search.Dispose();
                            var table = LoadArtistsIntoTable(artists);

                            this.ParentForm.BeginInvoke(new MethodInvoker(delegate()
                            {
                                CF_systemCommand(CF_Actions.HIDEINFO);
                                SwitchToTab(Tabs.Search, GroupingType.Artists, table, "Search", null, true);
                            }));
                        }
                        catch (Exception ex)
                        {
                            this.ParentForm.BeginInvoke(new MethodInvoker(() =>
                            {
                                CF_systemCommand(CF_Actions.HIDEINFO);
                                WriteError(ex);
                                CF_displayMessage(ex.Message);
                            }));
                        }
                    });
                }
            }
        }

        private void LoadPlaylistSearch()
        {
            if (CheckLoggedInAndOnline())
            {
                CFDialogParams searchDialogParams = new CFDialogParams("Search by playlist name");
                CFDialogResults results = new CFDialogResults();
                DialogResult result = CF_displayDialog(CF_Dialogs.OSK, searchDialogParams, results);
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    string searchText = results.resultvalue;
                    CF_systemCommand(CF_Actions.SHOWINFO, "Searching...");
                    ThreadPool.QueueUserWorkItem(delegate(object param)
                    {
                        try
                        {
                            var search = SpotifySession.SearchPlaylist(searchText, 0, 20);

                            SleepUntilTrue(() => search.IsComplete);

                            List<IPlaylist> playlists = new List<IPlaylist>();
                            foreach (var playlist in search.Playlists)
                            {
                                playlists.Add(playlist);
                            }
                            search.Dispose();

                            var table = LoadPlaylistsIntoTable(playlists);

                            this.ParentForm.BeginInvoke(new MethodInvoker(delegate()
                            {
                                CF_systemCommand(CF_Actions.HIDEINFO);
                                SwitchToTab(Tabs.Search, GroupingType.Playlists, table, "Search", null, true);
                            }));
                        }
                        catch (Exception ex)
                        {
                            this.ParentForm.BeginInvoke(new MethodInvoker(() =>
                            {
                                CF_systemCommand(CF_Actions.HIDEINFO);
                                WriteError(ex);
                                CF_displayMessage(ex.Message);
                            }));
                        }
                    });
                }
            }
        }

        private DataTable LoadTracksIntoTable(IEnumerable<ITrack> tracks)
        {
            SleepUntilTrue(() => tracks.All(t => t.IsLoaded));

            DataTable table = new DataTable();
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Artist", typeof(string));
            table.Columns.Add("Album", typeof(string));
            table.Columns.Add("Starred", typeof(string));
            table.Columns.Add("TrackObject", typeof(ITrack));

            foreach (var track in tracks.Where(t => t.IsAvailable && !t.IsPlaceholder))
            {
                var newRow = table.NewRow();
                newRow["Name"] = track.Name;
                newRow["Artist"] = GetArtistsString(track.Artists);
                newRow["Album"] = track.Album.Name;
                newRow["Starred"] = GetStarredStatusString(track.IsStarred);
                newRow["TrackObject"] = track;
                table.Rows.Add(newRow);
            }

            return table;
        }

        private string GetStarredStatusString(bool isStarred)
        {
            return isStarred ? "starred_true" : "starred_false";
        }

        private DataTable LoadAlbumsIntoTable(IEnumerable<IAlbum> albums, bool ignoreAvailability)
        {
            SleepUntilTrue(() => albums.All(a => a.IsLoaded));

            DataTable table = new DataTable();
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Artist", typeof(string));
            table.Columns.Add("AlbumObject", typeof(IAlbum));

            foreach (var album in albums.Where(a => ignoreAvailability ? true : a.IsAvailable))
            {
                var newRow = table.NewRow();
                newRow["Name"] = album.Name;
                newRow["Artist"] = album.Artist.Name;
                newRow["AlbumObject"] = album;
                table.Rows.Add(newRow);
            }

            return table;
        }

        private DataTable LoadArtistsIntoTable(IEnumerable<IArtist> artists)
        {
            SleepUntilTrue(() => artists.All(a => a.IsLoaded));

            DataTable table = new DataTable();
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("ArtistObject", typeof(IArtist));

            foreach (var artist in artists)
            {
                var newRow = table.NewRow();
                newRow["Name"] = artist.Name;
                newRow["ArtistObject"] = artist;
                table.Rows.Add(newRow);
            }

            return table;
        }

        private DataTable LoadPlaylistsIntoTable(IEnumerable<IPlaylist> playlists)
        {
            SleepUntilTrue(() => playlists.All(p => p.IsLoaded && !string.IsNullOrEmpty(p.Name)));

            DataTable table = new DataTable();
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("PlaylistObject", typeof(IPlaylist));
            table.Columns.Add("OfflineStatus", typeof(string));
            table.Columns.Add("DownloadingStatus", typeof(string));

            foreach (var playlist in playlists)
            {
                var newRow = table.NewRow();
                newRow["Name"] = playlist.Name;
                newRow["Description"] = playlist.Description;
                newRow["PlaylistObject"] = playlist;
                newRow["OfflineStatus"] = GetPlaylistStatusString(playlist);
                newRow["DownloadingStatus"] = GetDownloadingStatusString(playlist);
                table.Rows.Add(newRow);
            }

            return table;
        }

        private string GetDownloadingStatusString(IPlaylist playlist)
        {
            if(playlist.OfflineStatus == PlaylistOfflineStatus.Downloading)
            {
                int status = playlist.OfflineDownloadProgress;
                return status + "%";
            }
            else
                return string.Empty;
        }

        private string GetPlaylistStatusString(IPlaylist playlist)
        {
            switch (playlist.OfflineStatus)
            {
                case PlaylistOfflineStatus.Downloading:
                    return "downloading";
                case PlaylistOfflineStatus.No:
                    return "online";
                case PlaylistOfflineStatus.Waiting:
                    return "queued";
                case PlaylistOfflineStatus.Yes:
                    return "offline";
                default:
                    throw new Exception("Unrecognized playlist state");
            }
        }

        private const int THREAD_SLEEP_INTERVAL = 500;
        private const int RETRY_COUNT = 20;
        private delegate bool SleepUntilTrueDelegate();

        /// <summary>
        /// Waits in certain intervals until the method returns true.
        /// </summary>
        /// <param name="method">Method to periodically check</param>
        private void SleepUntilTrue(SleepUntilTrueDelegate method)
        {
            for (int i = 0; i < RETRY_COUNT; i++)
            {
                if (!method())
                    Thread.Sleep(THREAD_SLEEP_INTERVAL);
                else
                    return;
            }

            if (!method())
                throw new Exception("Operation timed out, please try again");
        }
    }
}
