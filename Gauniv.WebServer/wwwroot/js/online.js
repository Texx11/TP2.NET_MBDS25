"use strict";

(function () {
    // Création de la connexion au Hub "OnlineHub"
    const connection = new signalR.HubConnectionBuilder().withUrl("/online").build();

    // Lorsque le Hub envoie l'événement "UpdateUserStatus", mettre à jour l'élément HTML
    connection.on("UpdateUserStatus", function (users) {
        const container = document.getElementById("onlineUsersList");
        if (!container) return;
        container.innerHTML = "";
        if (users && users.length > 0) {
            let list = "<ul>";
            users.forEach(user => {
                list += `<li>${user.userName} (Connecté à ${new Date(user.connectedAt).toLocaleTimeString()})</li>`;
            });
            list += "</ul>";
            container.innerHTML = list;
        } else {
            container.innerHTML = "<p>Aucun utilisateur en ligne</p>";
        }
    });

    // Démarrage de la connexion
    connection.start()
        .then(function () {
            console.log("Connecté au hub OnlineHub");
            // Demander la liste actuelle des utilisateurs en ligne
            connection.invoke("GetOnlineUsers").catch(function (err) {
                console.error(err.toString());
            });
        })
        .catch(function (err) {
            console.error(err.toString());
        });
})();
