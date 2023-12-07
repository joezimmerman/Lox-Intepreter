namespace LOX{
    interface LoxCallable {
        int arity();
        Object call(Interpreter interpreter, List<Object> arguments);
    }
}