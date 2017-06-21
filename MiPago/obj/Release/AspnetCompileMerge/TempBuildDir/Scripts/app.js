function ViewModel() {
    var self = this;
    var tokenKey = 'accessToken';

    //resultado
    self.result = ko.observable();
    self.user = ko.observable();
   
    //Form registro de usuarios
    self.registerEmail = ko.observable();
    self.registerPassword = ko.observable();
    self.registerPassword2 = ko.observable();

    //Form login
    self.loginEmail = ko.observable();
    self.loginPassword = ko.observable();

    //Pago 
    self.AppOrdenId = ko.observable();
    self.Monto = ko.observable();


    //Muestra el error de la solicitud
    function showError(jqXHR) {
        self.result(jqXHR.status + ': ' + jqXHR.statusText);
    }

    //funcion para registrar
    self.register = function () {
        self.result('');

        var data = {
            Email: self.registerEmail(),
            Password: self.registerPassword(),
            ConfirmPassword: self.registerPassword2()
        };

        $.ajax({
            type: 'POST',
            url: '/api/Account/Register',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(data)
        }).done(function (data) {
            self.result("Done!");
        }).fail(showError);
    }

    //Login devuelve el token de loggeo
    self.login = function () {
        self.result('');

        var loginData = {
            grant_type: 'password',
            username: self.loginEmail(),
            password: self.loginPassword()
        };

        $.ajax({
            type: 'POST',
            url: '/Token',
            data: loginData
        }).done(function (data) {
            self.user(data.userName);
            //Token de accesso almacenado en cache.
            sessionStorage.setItem(tokenKey, data.access_token);
        }).fail(showError);
    }

    //logout Elimina el token en cache 
    self.logout = function () {
        self.user('');
        sessionStorage.removeItem(tokenKey)
    }

    //Consumiendo la api
    self.callApi = function () {
        self.result('');

        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }
        data = { AppOrdenId: "456344324", Monto: "20000" };

        $.ajax({
            type: 'POST',
            url: '/api/Pago',
            headers: headers,
            data: data
        }).done(function (data) {
            self.result(data.url_pago);
        }).fail(showError);
    }

    //Consumiendo la api
    self.callApi2 = function () {

        var data = {
            AppOrdenId: self.AppOrdenId(),
            Monto: self.Monto()
        };

        $.ajax({
            type: 'POST',
            url: '/api/Pago',
            data: data
        }).done(function (data) {
            window.location = data.UrlPago;
        }).fail(showError);
    }
}

var app = new ViewModel();
ko.applyBindings(app);