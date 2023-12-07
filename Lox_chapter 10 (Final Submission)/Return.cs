
namespace LOX {
    class Return: SystemException {
        public readonly Object value;

        public Return(Object value): base(null, null) {
            this.value = value;
        }
    }
}