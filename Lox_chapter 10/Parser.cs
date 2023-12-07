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
            return assignment();
        }

        private Expr assignment(){
            Expr expr = or();
            if(match(TokenType.EQUAL)){
                Token equals = previous();
                Expr value = assignment();
                if(expr is Expr.Variable variableExpr){
                    Token name = variableExpr.name;
                    return new Expr.Assign(name, value);
                }
                error(equals, "Invalid assignment target.");
            }
            return expr;
        }

        private Expr or() {
            Expr expr = and();
            while(match(TokenType.OR)){
                Token oper = previous();
                Expr right = and();
                expr = new Expr.Logical(expr, oper, right);
            }
            return expr;
        }

        private Expr and(){
            Expr expr = equality();
            while (match(TokenType.AND)) {
            Token oper = previous();
            Expr right = equality();
            expr = new Expr.Logical(expr, oper, right);
            }
            return expr;
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
            return call();
        }

        private Expr call(){
            Expr expr = primary();
            if(match(TokenType.LEFT_PAREN)){expr = finishCall(expr);}
            //might need else {break;}
            return expr;
        }

        private Expr finishCall(Expr callee){
            List<Expr> arguments = new List<Expr>();
            if(!check(TokenType.RIGHT_PAREN)){
                do {
                    if(arguments.Count >= 255){error(peek(), "Can't have more than 255 arguments.");}
                    arguments.Add(expression());
                } while (match(TokenType.COMMA));
            }
            Token paren = consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");
            return new Expr.Call(callee, paren, arguments);
        }
        Expr primary() {
            if (match(TokenType.FALSE)) return new Expr.Literal(false);
            if (match(TokenType.TRUE)) return new Expr.Literal(true);
            if (match(TokenType.NIL)) return new Expr.Literal(null!);
            if (match(TokenType.NUMBER, TokenType.STRING)) {return new Expr.Literal(previous().literal);}
            if (match(TokenType.IDENTIFIER)) {return new Expr.Variable(previous());}
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
        private Stmt statement(){
            if(match(TokenType.PRINT)) return printStatement();
            if(match(TokenType.RETURN)) return returnStatement();
            if(match(TokenType.WHILE)) return whileStatement();
            if(match(TokenType.LEFT_BRACE)) return new Stmt.Block(block());
            if(match(TokenType.FOR)) return forStatement();
            if(match(TokenType.IF)) return ifStatement();
            return expressionStatement();
        }
        private Stmt ifStatement() {
            consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
            Expr condition = expression();
            consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition."); 

            Stmt thenBranch = statement();
            Stmt elseBranch = null;
            if (match(TokenType.ELSE)) {
                elseBranch = statement();
            }
            return new Stmt.If(condition, thenBranch, elseBranch);
        }

        private Stmt forStatement(){
            consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");
            Stmt initializer;
            if (match(TokenType.SEMICOLON)) {
            initializer = null!;
            } else if (match(TokenType.VAR)) {
            initializer = varDeclaration();
            } else {
            initializer = expressionStatement();
            }
            Expr condition = null!;
            if (!check(TokenType.SEMICOLON)) {
            condition = expression();
            }
            consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");
            Expr increment = null!;
            if (!check(TokenType.RIGHT_PAREN)) {
            increment = expression();
            }
            consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses.");
            Stmt body = statement();
            if (increment != null) {
                List<Stmt> bodyList = new List<Stmt>(); 
                bodyList.Add(body);
                bodyList.Add(new Stmt.Expression(increment));
                body = new Stmt.Block(bodyList);
            }

            if (condition == null) condition = new Expr.Literal(true);
            body = new Stmt.While(condition, body);

            if (initializer != null) {
                List<Stmt> bodyList = new List<Stmt>();
                bodyList.Add(initializer);
                bodyList.Add(body);
                body = new Stmt.Block(bodyList);
            }
            return body;
        }

        private List<Stmt> block(){
            List<Stmt> statements = new List<Stmt>();
            while(!check(TokenType.RIGHT_BRACE) && !isAtEnd()) {statements.Add(declaration());}
            consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
            return statements;
        }

        private Stmt printStatement(){
            Expr value = expression();
            consume(TokenType.SEMICOLON, "Expect ';' after value.");
            return new Stmt.Print(value);
        }

        private Stmt returnStatement(){
            Token keyword = previous();
            Expr value = null!;
            if (!check(TokenType.SEMICOLON)) {value = expression();}
            consume(TokenType.SEMICOLON, "Expect ';' after return value.");
            return new Stmt.Return(keyword, value);
        }

        private Stmt expressionStatement() {
            Expr expr = expression();
            consume(TokenType.SEMICOLON, "Expect ';' after expression.");
            return new Stmt.Expression(expr);
        }

        private Stmt.Function function(String kind) {
            Token name = consume(TokenType.IDENTIFIER, "Expect " + kind + " name.");
            consume(TokenType.LEFT_PAREN, "Expect '(' after " + kind + " name.");
            List<Token> parameters = new List<Token>();
            if (!check(TokenType.RIGHT_PAREN)) {
                do {
                    if (parameters.Count >= 255) {error(peek(), "Can't have more than 255 parameters.");}
                    parameters.Add(consume(TokenType.IDENTIFIER, "Expect parameter name."));
                } while (match(TokenType.COMMA));
            }
            consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");
            consume(TokenType.LEFT_BRACE, "Expect '{' before " + kind + " body.");
            List<Stmt> body = block();
            return new Stmt.Function(name, parameters, body);
        }

        private Stmt varDeclaration(){
            Token name = consume(TokenType.IDENTIFIER, "Expect variable name.");
            Expr initializer = null!;
            if(match(TokenType.EQUAL)){
                initializer = expression();
            }
            consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
            return new Stmt.Var(name, initializer);
        }
        private Stmt whileStatement() {
            consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
            Expr condition = expression();
            consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");
            Stmt body = statement();
            return new Stmt.While(condition, body);
        }


        private Stmt declaration(){
            try{
                if(match(TokenType.FUN)) return function("function");
                if(match(TokenType.VAR)) return varDeclaration();
                return statement();
            } catch (ParseError error){
                synchronize();
                return null!;
            }
        }

        public List<Stmt> parse(){
            List<Stmt> statements = new List<Stmt>();
            while (!isAtEnd()){
                statements.Add(declaration());
            }
            return statements;
        }
    }
}