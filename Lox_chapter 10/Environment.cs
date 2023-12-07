namespace LOX{
    public class Environment{
        public readonly Environment? enclosing;
        private readonly Dictionary<string, object> values = new Dictionary<string, object>();

        public Environment() {enclosing = null;}
        public Environment(Environment enclosing) {this.enclosing = enclosing;}
        public Object get(Token name){
            if(values.ContainsKey(name.lexeme)){return values[name.lexeme];}
            if(enclosing != null) return enclosing.get(name);
            throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
        }

        public void assign(Token name, Object value){
            if(values.ContainsKey(name.lexeme)) {values[name.lexeme] = value; return;}
            if(enclosing != null) {enclosing.assign(name, value); return;}
            throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
        }

        public void define(String name, Object value){
            values[name] = value;
        }

    }
}