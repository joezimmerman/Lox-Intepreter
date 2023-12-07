using System.Diagnostics;

namespace LOX{
    public class Interpreter : Expr.IVisitor<Object>, Stmt.IVisitor<Object> {
        
        //Up to this point I have implemented the entire Stmt class, I did this by looking at the final github and implementing the entire class. I couldn't figure out the GenerateAST.
        private Environment environment = new Environment();
        public Object visitLiteralExpr(Expr.Literal expr){
            return expr.value;
        }

        public Object visitCallExpr(Expr.Call expr){
            Object callee = evaluate(expr.callee);
            List<Object> arguments = new List<Object>();
            foreach (Expr argument in expr.arguments) {
                arguments.Add(evaluate(argument));
            }
            if (!(callee is LoxCallable)) { throw new RuntimeError(expr.paren, "Can only call functions and classes.");}
            LoxCallable function = (LoxCallable)callee;
            if (arguments.Count() != function.arity()) {throw new RuntimeError(expr.paren, "Expected " + function.arity() + " arguments but got " + arguments.Count() + ".");}
            return function.call(this, arguments);
        }

        public Object visitLogicalExpr(Expr.Logical expr){
            Object left = evaluate(expr.left);
            if (expr.oper.type == TokenType.OR) {
            if (isTruthy(left)) return left;
            } else {
            if (!isTruthy(left)) return left;
            }
            return evaluate(expr.right);
        }

        public Object visitGroupingExpr(Expr.Grouping expr){
            return evaluate(expr.expression);
        }

        private Object evaluate(Expr expr){
            return expr.accept(this);
        }

        public Object visitExpressionStmt(Stmt.Expression stmt){
            evaluate(stmt.expression);
            return null!;
        }



        public Object visitReturnStmt(Stmt.Return stmt){
            Object value = null!;
            if (stmt.value != null) value = evaluate(stmt.value);

            throw new Return(value);
        }

        public Object visitWhileStmt(Stmt.While stmt) {
            while (isTruthy(evaluate(stmt.condition))) {execute(stmt.body);}
            return null!;
        }

        public Object visitIfStmt(Stmt.If stmt){
            if(isTruthy(evaluate(stmt.condition))) {execute(stmt.thenBranch);}
            else if (stmt.elseBranch != null){execute(stmt.elseBranch);}
            return null!;
        }

        public Object visitPrintStmt(Stmt.Print stmt){
            Object value = evaluate(stmt.expression);
            Console.WriteLine(stringify(value));
            return null!;
        }
        public Object visitVarStmt(Stmt.Var stmt){
            Object value = null!;
            if(stmt.initializer != null){
                value = evaluate(stmt.initializer);
            }
            environment.define(stmt.name.lexeme, value);
            return null!;
        }

        public Object visitFunctionStmt(Stmt.Function stmt){
            LoxFunction function = new LoxFunction(stmt, environment);
            environment.define(stmt.name.lexeme, function);
            return null!;
        }


        public Object visitAssignExpr(Expr.Assign expr){
            Object value = evaluate(expr.value);
            environment.assign(expr.name, value);
            return value;
        }

        public Object visitUnaryExpr(Expr.Unary expr){
            Object right = evaluate(expr.right);
            switch (expr.oper.type){
                case TokenType.BANG:
                    return !isTruthy(right);
                case TokenType.MINUS:
                    checkNumberOperand(expr.oper, right);
                    return -(double)right;
            }
            return null!;
        }

        public Object visitVariableExpr(Expr.Variable expr){
            return environment.get(expr.name);
        }

        

        private void checkNumberOperand(Token oper, Object operand){
            if(operand is double) return;
            throw new RuntimeError(oper, "Operand must be a number.");
        }

        private bool isTruthy(Object o){
            if(o == null) return false;
            if(o is bool booleanVal) return booleanVal;
            return true;
        }

        public Object visitBinaryExpr(Expr.Binary expr){
            Object left = evaluate(expr.left);
            Object right = evaluate(expr.right);
            switch(expr.oper.type){
                case TokenType.GREATER:
                    checkNumberOperands(expr.oper, left, right);
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    checkNumberOperands(expr.oper, left, right);
                    return (double)left >= (double)right;
                case TokenType.LESS:
                    checkNumberOperands(expr.oper, left, right);
                    return (double)left < (double)right;
                case TokenType.LESS_EQUAL:
                    checkNumberOperands(expr.oper, left, right);
                    return (double)left <= (double)right;
                case TokenType.MINUS:
                    checkNumberOperands(expr.oper, left, right);
                    return (double)left - (double)right;
                case TokenType.PLUS:
                    if(left is double && right is double) {return (double)left + (double)right;}
                    if(left is string && right is string) {return (string)left + (string)right;}
                    throw new RuntimeError(expr.oper, "Operands must be two numbers or two strings.");
                case TokenType.SLASH:
                    checkNumberOperands(expr.oper, left, right);
                    return (double)left / (double)right;
                case TokenType.STAR:
                    checkNumberOperands(expr.oper, left, right);
                    return (double)left * (double)right;
                case TokenType.BANG_EQUAL: return !isEqual(left, right);
                case TokenType.EQUAL_EQUAL: return isEqual(left, right);
            }
            return null!;
        }
        private void checkNumberOperands(Token oper, Object left, Object right) {
            if (left is double && right is double) return;
            throw new RuntimeError(oper, "Operands must be numbers.");
        }
        private bool isEqual(Object a, Object b){
            if (a == null && b == null) return true;
            if (a == null) return false;
            return a.Equals(b);
        }
        public void interpret(List<Stmt> statements){
            try{
                foreach(Stmt statement in statements){
                    execute(statement);
                }
            }
            catch (RuntimeError error){Lox.runtimeError(error);}
        }
        private String stringify(Object obj) {
            if (obj == null) return "nil";
            if (obj is Double) {
                String text = obj.ToString();
                if (text.EndsWith(".0")) {
                    text = text.Substring(0, text.Length - 2);  //  Double check string not cutting off chars
                }
                return text;
            }
            return obj.ToString();
        }
        private void execute(Stmt stmt){
            stmt.accept(this);
        }
        public Object visitBlockStmt(Stmt.Block stmt){
            executeBlock(stmt.statements, new Environment(environment));
            return null!;
        }
        public void executeBlock(List<Stmt> statements, Environment environment){
            Environment previous = this.environment;
            try {
            this.environment = environment;

            foreach (Stmt statement in statements) {
                execute(statement);
            }
            } finally {
            this.environment = previous;
            }
        }

    }

}