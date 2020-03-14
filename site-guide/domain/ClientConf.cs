namespace site_guide.domain
{
    class ClientConf
    {
        // 本地 DNS 网关地址, 如: 192.168.1.1
        public string localDnsAddress;

        // 更新本地IP的最短时间间隔
        public int minTime;

        // 更新本地IP的最长时间间隔
        public int maxTime;

    }
}
