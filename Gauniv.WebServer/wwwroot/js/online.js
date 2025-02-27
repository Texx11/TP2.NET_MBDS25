"use strict";

(function () {

    // Variables pour stocker la liste complète et la liste des connectés
    let allUsers = [];
    let connectedUserIds = [];
    let onlineDetails = {}; // mapping de userId -> connectedAt

    // Création de la connexion au Hub "OnlineHub"
    const connection = new signalR.HubConnectionBuilder().withUrl("/online").build();

    function refreshDisplay() {
        const container = document.getElementById("onlineUsersList");
        if (!container) return;

        container.innerHTML = ""; // On vide d'abord

        if (allUsers.length === 0) {
            container.innerHTML = "<p>Aucun utilisateur</p>";
            return;
        }

        // Séparation en ligne et hors ligne
        const online = allUsers.filter(u => connectedUserIds.includes(u.id));
        const offline = allUsers.filter(u => !connectedUserIds.includes(u.id));

        let html = "";
        // Bloc des utilisateurs en ligne
        html += `<h3>Utilisateurs en ligne</h3>`;
        if (online.length > 0) {
            html += `<div class="users-container online-container" style="margin-bottom: 30px;">`;
            online.forEach(user => {
                let connectedTime = onlineDetails[user.id]
                    ? new Date(onlineDetails[user.id]).toLocaleTimeString()
                    : "inconnue";
                html += `
                    <div class="user-card">
                        <div class="status">
                            <span class="status-indicator"></span>
                            <h4>${user.userName}</h4>
                        </div>
                        <p>Connecté à ${connectedTime}</p>
                    </div>
                `;
            });
            html += `</div>`;
        } else {
            html += `<p>Aucun utilisateur en ligne</p>`;
        }

        // Bloc des utilisateurs hors ligne
        html += `<h3>Utilisateurs hors ligne</h3>`;
        if (offline.length > 0) {
            html += `<div class="users-container offline-container">`;
            offline.forEach(user => {
                html += `
                    <div class="user-card">
                        <h4>${user.userName}</h4>
                        <p>(Hors ligne)</p>
                    </div>
                `;
            });
            html += `</div>`;
        } else {
            html += `<p>Aucun utilisateur hors ligne</p>`;
        }

        container.innerHTML = html;
    }

    // Lorsque le Hub envoie la liste de tous les utilisateurs (hors admin)
    connection.on("AllUsers", function (users) {
        allUsers = users;
        refreshDisplay();
    });

    // Lorsque le Hub envoie "UpdateUserStatus", on récupère la liste des connectés et leurs infos
    connection.on("UpdateUserStatus", function (connectedList) {
        // connectedList est une liste d'objets OnlineStatus
        connectedUserIds = connectedList.map(u => u.userId);
        onlineDetails = {};
        connectedList.forEach(u => {
            onlineDetails[u.userId] = u.connectedAt;
        });
        refreshDisplay();
    });

    // Démarrage de la connexion
    connection.start()
        .then(function () {
            console.log("Connecté au hub OnlineHub");

            // Demande la liste de tous les utilisateurs (hors admin)
            connection.invoke("GetAllUsersExceptAdmins")
                .catch(function (err) { console.error(err.toString()); });

            // Demande la liste actuelle des utilisateurs en ligne
            connection.invoke("GetOnlineUsers")
                .catch(function (err) { console.error(err.toString()); });
        })
        .catch(function (err) {
            console.error(err.toString());
        });
})();
