OwinAuthWithNancy
=================

「OWINでForm認証」 + 「ASP.NET Identity」で独自認証する + 「Nancy+OWINでセキュアなページを作成する」 のサンプル


## 1.非セキュアページ

http://localhost:55127

http://localhost:55127/hoge


## 2.ログイン

### リダイレクト先未指定

http://localhost:55127/login

ログイン後、/secure へリダイレクトされる

### リダイレクト先を指定

http://localhost:55127/login?RedirectUrl=/hoge

ログイン後、 /hoge へリダイレクトされる


## 3.セキュアページ

http://localhost:55127/secure

ログイン時に指定したユーザー名が表示される。未ログイン時にアクセスするとException発生する。


## 4.ログアウト

### リダイレクト先未指定

http://localhost:55127/logout

ログアウト後、 / へリダイレクトされる

### リダイレクト先を指定

http://localhost:55127/logout?RedirectUrl=/hoge

ログアウト後、 /hoge へリダイレクトされる

