namespace site_guide.domain
{
    class ApiResponse<T>
    {
        public int code;

        public string msg;

        public T data;
    }
}
