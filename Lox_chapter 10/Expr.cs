namespace LOX {
    public abstract class Expr {
        public interface IVisitor<R> {
            R visitAssignExpr(Assign expr);
            R visitBinaryExpr(Binary expr);
            R visitCallExpr(Call expr);
            R visitGroupingExpr(Grouping expr);
            R visitLiteralExpr(Literal expr);
            R visitLogicalExpr(Logical expr);
            R visitUnaryExpr(Unary expr);
            R visitVariableExpr(Variable expr);
        }
        public class Assign : Expr {
            public Assign(Token name, Expr value) {
                this.name = name;
                this.value = value;
            }

            public override R accept<R>(IVisitor<R> visitor) {   //  Check protected modified for each class and override
                return visitor.visitAssignExpr(this);
            }   

            public Token name;
            public Expr value;
        } 
        public class Binary : Expr {
            public Binary(Expr left, Token oper, Expr right) {
                this.left = left;
                this.oper = oper;
                this.right = right;
            }

            public override R accept<R>(IVisitor<R> visitor) {
                return visitor.visitBinaryExpr(this);
            }
            public readonly Expr left;
            public readonly Token oper;
            public readonly Expr right;
        }
        public class Call : Expr {
            public Call(Expr callee, Token paren, List<Expr> arguments) {
                this.callee = callee;
                this.paren = paren;
                this.arguments = arguments;
            }

            public override R accept<R>(IVisitor<R> visitor) {
                return visitor.visitCallExpr(this);
            }

            public readonly Expr callee;
            public readonly Token paren;
            public readonly List<Expr> arguments;
        }
        public class Grouping : Expr {
            public Grouping(Expr expression) {
                this.expression = expression;
            }

            public override R accept<R>(IVisitor<R> visitor) {
                return visitor.visitGroupingExpr(this);
            }

            public readonly Expr expression;
        }

        public class Literal : Expr {
            public Literal(Object value) {
                this.value = value;
            }

            public override R accept<R>(IVisitor<R> visitor) {
                return visitor.visitLiteralExpr(this);
            }

            public readonly Object value;
        }
        
              public class Logical: Expr {
                public Logical(Expr left, Token oper, Expr right) {
                  this.left = left;
                  this.oper = oper;
                  this.right = right;
                }

                public override R accept<R>(IVisitor<R> visitor) {
                  return visitor.visitLogicalExpr(this);
                }

                public readonly Expr left;
                public readonly Token oper;
                public readonly Expr right;
              }
        public class Unary : Expr {
            public Unary(Token oper, Expr right) {
                this.oper = oper;
                this.right = right;
            }

            public override R accept<R>(IVisitor<R> visitor) {
                return visitor.visitUnaryExpr(this);
            }

            public readonly Token oper;
            public readonly Expr right;
        }

        public class Variable : Expr {
            public Variable(Token name) {
                this.name = name;
            }

            public override R accept<R>(IVisitor<R> visitor) {
                return visitor.visitVariableExpr(this);
            }

            public Token name;
        }
        public abstract R accept<R>(IVisitor<R> visitor);
    }
}