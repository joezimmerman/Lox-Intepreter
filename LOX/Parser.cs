using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LOX {
        public class ParseError : Exception { public ParseError(string message = null!) : base(message) { } }    
        public class Parser{
        private readonly List<Token> tokens; 
        private int current = 0;
        public Parser(List<Token> tokens){
            this.tokens = tokens;
        }
        private Expr expression(){
            return equality();
        }

        private Expr equality(){
            Expr expr = comparison();
            while (match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL)){
                Token oper = previous(); //might need an @ before the operator, could be operator instead of oper?
                Expr right = comparison();
                expr = new Expr.Binary(expr, oper, right); //might need an @ in front of oper
            }
            return expr;
        }

        private bool match(params TokenType[] types){
            foreach (var type in types){
                if(check(type)){advance(); return true;}
            }
            return false;
        }

        private bool check(TokenType type){
            if(isAtEnd()) return false;
            return peek().type == type;
        }

        private Token advance(){
            if (!isAtEnd()) current++;
            return previous();
        }
        private bool isAtEnd(){
            return peek().type == TokenType.EOF;
        }
        private Token peek(){
            return tokens[current];
        }

        private Token previous(){
            return tokens[current-1];
        }

        private Expr comparison(){
            Expr expr = term();
            while(match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL)){
                Token oper = previous();
                Expr right = term();
                expr = new Expr.Binary(expr, oper, right);
            }
            return expr;
        }

        private Expr term(){
            Expr expr = factor();
            while(match(TokenType.MINUS, TokenType.PLUS)){
                Token oper = previous();
                Expr right = factor();
                expr = new Expr.Binary(expr, oper, right);
            }
            return expr;
        }
        private Expr factor(){
            Expr expr = unary();
            while(match(TokenType.SLASH, TokenType.STAR)){
                Token oper = previous();
                Expr right = unary();
                expr = new Expr.Binary(expr, oper, right);
            }
            return expr;
        }
        private Expr unary(){
            if(match(TokenType.BANG, TokenType.MINUS)){
                Token oper = previous();
                Expr right = unary();
                return new Expr.Unary(oper, right);
            }
            return primary();
        }
        Expr primary() {
            if (match(TokenType.FALSE)) return new Expr.Literal(false);
            if (match(TokenType.TRUE)) return new Expr.Literal(true);
            if (match(TokenType.NIL)) return new Expr.Literal(null!);

            if (match(TokenType.NUMBER, TokenType.STRING)) {return new Expr.Literal(previous().literal);}

            if (match(TokenType.LEFT_PAREN)) {
                Expr expr = expression();
                consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
                return new Expr.Grouping(expr);
            }
            throw error(peek(), "Expect expression.");
        }
        public Token consume(TokenType type, string message){
            if(check(type)) return advance();
            throw error(peek(), message);
        }

        public ParseError error(Token token, string message){
            Lox.error(token, message);
            return new ParseError();
        }
        public void synchronize(){
            advance();
            while(!isAtEnd()){
                if(previous().type == TokenType.SEMICOLON) return;
                switch (peek().type){
                    case TokenType.CLASS:
                    case TokenType.FUN:
                    case TokenType.VAR:
                    case TokenType.FOR:
                    case TokenType.IF:
                    case TokenType.WHILE:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                        return;
                }
                advance();
            }
        }
        public Expr parse(){
            try {return expression();}
            catch (ParseError) {return null!;}
        }

    }
}