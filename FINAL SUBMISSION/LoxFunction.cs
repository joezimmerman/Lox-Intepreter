namespace LOX{
    class LoxFunction : LoxCallable {
        private readonly Stmt.Function declaration;
        private readonly Environment closure;
        public LoxFunction(Stmt.Function declaration, Environment closure) {
            this.closure = closure;
            this.declaration = declaration;
        }

        public String toString() {
            return "<fn " + declaration.name.lexeme + ">";
        }

        public int arity() {
            return declaration.parameters.Count;
        }

        public Object call(Interpreter interpreter, List<Object> arguments) {
            Environment environment = new Environment(closure);
            for (int i = 0; i < declaration.parameters.Count; i++) {
                environment.define(declaration.parameters[i].lexeme,
                    arguments[i]);
            }

            try {
                interpreter.executeBlock(declaration.body, environment);
            }
            catch (Return returnValue) {
                return returnValue.value;
            }
            return null!;
        }
    }
}