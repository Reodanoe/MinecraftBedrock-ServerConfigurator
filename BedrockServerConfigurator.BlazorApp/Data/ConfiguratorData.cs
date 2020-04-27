using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BedrockServerConfigurator.BlazorApp.Data
{
    public class ConfiguratorData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int percentDownloaded;

        /// <summary>
        /// How much percent of template server has been downloaded so far
        /// </summary>
        public int PercentDownloaded
        {
            get { return percentDownloaded; }
            set
            {
                percentDownloaded = value;
                CallPropertyChanged();
            }
        }

        private bool newDownloadStarted;

        /// <summary>
        /// If template server download started
        /// </summary>
        public bool NewDownloadStarted
        {
            get { return newDownloadStarted; }
            set
            {
                newDownloadStarted = value;
                CallPropertyChanged();
            }
        }

        private bool serverDownloaded;

        /// <summary>
        /// If template server has been downloaded
        /// </summary>
        public bool ServerDownloaded
        {
            get { return serverDownloaded; }
            set
            {
                serverDownloaded = value;
                CallPropertyChanged();
            }
        }

        /// <summary>
        /// If template server is currently downloading
        /// </summary>
        public bool IsDownloading => NewDownloadStarted && !ServerDownloaded;

        private bool creatingNewServer;

        /// <summary>
        /// True if creating new server (copying template server)
        /// </summary>
        public bool CreatingNewServer
        {
            get { return creatingNewServer; }
            set
            {
                creatingNewServer = value;
                CallPropertyChanged();
            }
        }

        /// <summary>
        /// int is ID of server, ServerData holds data on individual server components
        /// </summary>
        public Dictionary<int, ServerData> AllServerData { get; set; } = new Dictionary<int, ServerData>();

        private void CallPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
