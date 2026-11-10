/**
 * NihongoAcademy - Unified Notifications & Direct Messages Hub
 */
(function () {
    var badge = document.getElementById("naNotifBadge");
    var list = document.getElementById("naNotifList");
    var markAllBtn = document.getElementById("naMarkAllReadBtn");

    var tabAlerts = document.getElementById("btnTabAlerts");
    var tabMessages = document.getElementById("btnTabMessages");
    var panelAlerts = document.getElementById("panelAlerts");
    var panelMessages = document.getElementById("panelMessages");

    var tabAlertsCount = document.getElementById("tabAlertsCount");
    var tabDmsCount = document.getElementById("tabDmsCount");
    var lblBuddiesTotal = document.getElementById("lblBuddiesTotal");
    var buddiesStrip = document.getElementById("naBuddiesStrip");
    var dmList = document.getElementById("naDmList");

    var activeTab = "alerts";

    function getCsrfToken() {
        var tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : "";
    }

    // Messages echo learner-authored content (comment bodies, post titles, DM text),
    // so every server value that reaches a string of HTML below goes through here.
    function escapeHtml(value) {
        return String(value == null ? "" : value)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

    function timeAgo(dateString) {
        if (!dateString) return "";
        var d = new Date(dateString);
        var diff = Math.floor((new Date() - d) / 1000);
        if (diff < 60) return "just now";
        if (diff < 3600) return Math.floor(diff / 60) + "m ago";
        if (diff < 86400) return Math.floor(diff / 3600) + "h ago";
        return Math.floor(diff / 86400) + "d ago";
    }

    function getIcon(type) {
        if (type === "like") return '<i class="bi bi-heart-fill text-danger"></i>';
        if (type === "reply") return '<i class="bi bi-reply-fill text-primary"></i>';
        if (type === "buddy") return '<i class="bi bi-flower1 text-warning"></i>';
        if (type === "cheer") return '<i class="bi bi-stars text-warning"></i>';
        if (type === "message") return '<i class="bi bi-chat-quote-fill text-info"></i>';
        return '<i class="bi bi-bell-fill text-secondary"></i>';
    }

    function createAvatarElement(avatarMode, avatarImagePath, avatarJson, displayName, size) {
        size = size || 34;
        var initial = (displayName || "U").charAt(0).toUpperCase();

        if (avatarMode === "uploaded" && avatarImagePath) {
            var img = document.createElement("img");
            img.src = avatarImagePath;
            img.alt = displayName || "";
            img.style.width = size + "px";
            img.style.height = size + "px";
            img.style.borderRadius = "50%";
            img.style.objectFit = "cover";
            img.style.flexShrink = "0";
            return img;
        }

        var div = document.createElement("div");
        div.style.width = size + "px";
        div.style.height = size + "px";
        div.style.borderRadius = "50%";
        div.style.overflow = "hidden";
        div.style.flexShrink = "0";
        div.style.display = "inline-flex";
        div.style.alignItems = "center";
        div.style.justifyContent = "center";
        div.style.background = "#ee96a4";
        div.style.color = "#fff";
        div.style.fontWeight = "bold";
        div.style.fontSize = Math.max(11, Math.floor(size * 0.42)) + "px";

        if (window.AvatarBuilder && avatarJson) {
            try {
                var config = JSON.parse(avatarJson);
                div.dataset.size = size;
                AvatarBuilder.renderInto(div, config, size);
                return div;
            } catch (e) {}
        }

        div.textContent = initial;
        return div;
    }

    // ===== Tab Switcher =====
    window.switchNotifTab = function (tab, event) {
        if (event) {
            event.preventDefault();
            event.stopPropagation();
        }
        activeTab = tab;
        var tAlerts = document.getElementById("btnTabAlerts");
        var tMessages = document.getElementById("btnTabMessages");
        var pAlerts = document.getElementById("panelAlerts");
        var pMessages = document.getElementById("panelMessages");

        if (tab === "alerts") {
            if (tAlerts) {
                tAlerts.classList.add("active");
                tAlerts.classList.remove("text-muted");
            }
            if (tMessages) {
                tMessages.classList.remove("active");
                tMessages.classList.add("text-muted");
            }
            if (pAlerts) pAlerts.style.display = "block";
            if (pMessages) pMessages.style.display = "none";
        } else {
            if (tMessages) {
                tMessages.classList.add("active");
                tMessages.classList.remove("text-muted");
            }
            if (tAlerts) {
                tAlerts.classList.remove("active");
                tAlerts.classList.add("text-muted");
            }
            if (pAlerts) pAlerts.style.display = "none";
            if (pMessages) pMessages.style.display = "block";
            loadChats();
        }
    };

    // Prevent Bootstrap from closing dropdown when interacting inside it
    var notifDropdownEl = document.querySelector(".na-notif-dropdown");
    if (notifDropdownEl) {
        notifDropdownEl.addEventListener("click", function (e) {
            var link = e.target.closest("a");
            if (link && link.getAttribute("href") && link.getAttribute("href") !== "#") {
                return; // Let links navigate normally
            }
            e.stopPropagation();
        });
    }

    // ===== Load System Alerts =====
    function loadNotifications() {
        badge = badge || document.getElementById("naNotifBadge");
        list = list || document.getElementById("naNotifList");
        if (!badge || !list) return;

        fetch("/Notifications/Recent")
            .then(function (res) { return res.json(); })
            .then(function (data) {
                if (!data.success) return;

                // Total bell badge
                if (data.unreadCount > 0) {
                    badge.textContent = data.unreadCount > 9 ? "9+" : data.unreadCount;
                    badge.style.display = "inline-flex";
                } else {
                    badge.style.display = "none";
                }

                // Alerts tab counter
                if (tabAlertsCount) {
                    if (data.unreadNotifsCount > 0) {
                        tabAlertsCount.textContent = data.unreadNotifsCount;
                        tabAlertsCount.style.display = "inline-block";
                    } else {
                        tabAlertsCount.style.display = "none";
                    }
                }

                // Messages tab counter
                if (tabDmsCount) {
                    if (data.unreadDmsCount > 0) {
                        tabDmsCount.textContent = data.unreadDmsCount;
                        tabDmsCount.style.display = "inline-block";
                    } else {
                        tabDmsCount.style.display = "none";
                    }
                }

                // Render list
                if (!data.notifications || data.notifications.length === 0) {
                    list.innerHTML = '<div class="text-center text-muted py-4 small">No notifications yet.</div>';
                    return;
                }

                var html = "";
                data.notifications.forEach(function (n) {
                    var unreadCls = n.isRead ? "" : "na-notif-item--unread";
                    html +=
                        '<a href="' + escapeHtml(n.targetUrl) + '" class="na-notif-item ' + unreadCls + '" data-id="' + escapeHtml(n.id) + '">' +
                            '<div class="na-notif-icon">' + getIcon(n.type) + '</div>' +
                            '<div class="na-notif-content">' +
                                '<div class="na-notif-msg">' + escapeHtml(n.message) + '</div>' +
                                '<div class="na-notif-time">' + timeAgo(n.createdDate) + '</div>' +
                            '</div>' +
                        '</a>';
                });
                list.innerHTML = html;

                list.querySelectorAll(".na-notif-item").forEach(function (el) {
                    el.addEventListener("click", function () {
                        var notifId = el.getAttribute("data-id");
                        fetch("/Notifications/MarkAsRead", {
                            method: "POST",
                            headers: {
                                "Content-Type": "application/json",
                                "RequestVerificationToken": getCsrfToken()
                            },
                            body: JSON.stringify({ notificationId: parseInt(notifId, 10) })
                        });
                    });
                });
            })
            .catch(function () {});
    }

    // ===== Load Direct Messages =====
    function loadChats() {
        dmList = dmList || document.getElementById("naDmList");
        if (!dmList) return;

        fetch("/Notifications/RecentChatsAndBuddies")
            .then(function (res) { return res.json(); })
            .then(function (data) {
                if (!data.success) return;

                // Update unread DMs count badge
                if (tabDmsCount) {
                    if (data.unreadDmsCount > 0) {
                        tabDmsCount.textContent = data.unreadDmsCount;
                        tabDmsCount.style.display = "inline-block";
                    } else {
                        tabDmsCount.style.display = "none";
                    }
                }

                dmList.innerHTML = "";
                var chats = data.recentChats || [];

                if (chats.length === 0) {
                    dmList.innerHTML = 
                        '<div class="text-center text-muted py-4 small px-3">' +
                            '<i class="bi bi-chat-heart text-na-primary d-block fs-3 mb-2"></i>' +
                            'No direct messages yet.<br>' +
                            '<span style="font-size:0.75rem;">Visit any member\'s profile to start a conversation! 🌸</span>' +
                        '</div>';
                    return;
                }

                chats.forEach(function (c) {
                    var item = document.createElement("div");
                    item.className = "na-dm-item d-flex align-items-center gap-2 p-2 border-bottom position-relative";
                    item.style.cursor = "pointer";
                    item.style.transition = "background 0.15s ease";
                    if (c.unreadCount > 0) {
                        item.style.background = "rgba(238, 150, 164, 0.08)";
                    }

                    item.addEventListener("mouseenter", function () { item.style.background = "#F8FAFC"; });
                    item.addEventListener("mouseleave", function () {
                        item.style.background = c.unreadCount > 0 ? "rgba(238, 150, 164, 0.08)" : "transparent";
                    });

                    var av = createAvatarElement(c.avatarMode, c.avatarImagePath, c.avatarJson, c.displayName, 36);
                    item.appendChild(av);

                    var info = document.createElement("div");
                    info.className = "flex-grow-1 min-w-0";

                    var topRow = document.createElement("div");
                    topRow.className = "d-flex justify-content-between align-items-center";

                    var name = document.createElement("strong");
                    name.className = "text-truncate d-block text-dark";
                    name.style.fontSize = "0.82rem";
                    name.textContent = c.displayName || c.userName;
                    if (c.isMutual) {
                        name.textContent += " 🌸";
                    }
                    topRow.appendChild(name);

                    var time = document.createElement("small");
                    time.className = "text-muted flex-shrink-0 ms-1";
                    time.style.fontSize = "0.68rem";
                    time.textContent = c.sentAtFormatted;
                    topRow.appendChild(time);

                    info.appendChild(topRow);

                    var bottomRow = document.createElement("div");
                    bottomRow.className = "d-flex justify-content-between align-items-center mt-1";

                    var snippet = document.createElement("span");
                    snippet.className = "text-truncate text-muted d-block";
                    snippet.style.fontSize = "0.75rem";
                    snippet.textContent = (c.isMine ? "You: " : "") + c.lastMessage;
                    bottomRow.appendChild(snippet);

                    if (c.unreadCount > 0) {
                        var badgeEl = document.createElement("span");
                        badgeEl.className = "badge bg-danger rounded-pill flex-shrink-0 ms-1";
                        badgeEl.style.fontSize = "0.65rem";
                        badgeEl.textContent = c.unreadCount;
                        bottomRow.appendChild(badgeEl);
                    }

                    info.appendChild(bottomRow);
                    item.appendChild(info);

                    item.addEventListener("click", function (ev) {
                        ev.stopPropagation();
                        if (window.GlobalStudyChat) {
                            window.GlobalStudyChat.open(c.userId, c.displayName, c.userName, c.isMutual);
                        }
                    });

                    dmList.appendChild(item);
                });
            })
            .catch(function () {});
    }

    // ===== Global Study Chat Drawer Controller =====
    var activeChatUserId = null;

    window.GlobalStudyChat = {
        open: function (targetUserId, displayName, userName, isMutual) {
            activeChatUserId = targetUserId;
            var box = document.getElementById("globalStudyChatBox");
            if (!box) {
                // If not signed in, redirect to login
                window.location.href = "/Identity/Account/Login?returnUrl=" + encodeURIComponent(window.location.pathname + window.location.search);
                return;
            }

            var titleEl = document.getElementById("globalChatTitle");
            var subEl = document.getElementById("globalChatSubtitle");
            var profileLink = document.getElementById("globalChatProfileLink");
            var avEl = document.getElementById("globalChatAvatar");

            if (titleEl) titleEl.textContent = displayName || userName || "Study Buddy";
            if (subEl) {
                subEl.textContent = "@" + (userName || "learner") + (isMutual ? " · Mutual Buddy 🌸" : " · Study Buddy");
            }
            if (profileLink) {
                profileLink.href = "/Community/UserProfile/" + encodeURIComponent(targetUserId);
            }
            if (avEl) {
                avEl.textContent = (displayName || userName || "U").charAt(0).toUpperCase();
            }

            box.style.display = "flex";
            box.style.zIndex = "1080";
            if (window.naKeys && typeof window.naKeys.toast === "function") {
                window.naKeys.toast("Chatting with " + (displayName || userName || "learner"), "💬");
            }
            window.GlobalStudyChat.loadHistory(targetUserId);

            setTimeout(function () {
                var input = document.getElementById("globalChatInput");
                if (input) input.focus();
            }, 100);
        },

        close: function () {
            var box = document.getElementById("globalStudyChatBox");
            if (box) box.style.display = "none";
            activeChatUserId = null;
        },

        loadHistory: function (targetUserId) {
            var thread = document.getElementById("globalChatThread");
            if (!thread) return;

            fetch("/Community/GetConversation?targetUserId=" + encodeURIComponent(targetUserId), {
                method: "POST",
                headers: { "RequestVerificationToken": getCsrfToken() }
            })
                .then(function (res) { return res.json(); })
                .then(function (data) {
                    if (!data.success || !data.messages) return;
                    thread.innerHTML = "";
                    if (data.messages.length === 0) {
                        thread.innerHTML = 
                            '<div class="text-center text-muted small py-4" id="globalChatEmpty">' +
                                '<i class="bi bi-chat-square-heart text-na-primary d-block fs-3 mb-1"></i>' +
                                'Start a study conversation! Say hello or practice Japanese together.' +
                            '</div>';
                        return;
                    }
                    data.messages.forEach(function (msg) {
                        window.GlobalStudyChat.appendBubble(msg.content, msg.isMine, msg.sentAtFormatted);
                    });
                    thread.scrollTop = thread.scrollHeight;

                    // Also reload counts
                    loadNotifications();
                    if (activeTab === "messages") {
                        loadChats();
                    }
                })
                .catch(function () {});
        },

        appendBubble: function (text, isMine, timeStr) {
            var thread = document.getElementById("globalChatThread");
            if (!thread) return;

            var empty = document.getElementById("globalChatEmpty");
            if (empty) empty.remove();

            var bubbleWrapper = document.createElement("div");
            bubbleWrapper.style.display = "flex";
            bubbleWrapper.style.flexDirection = "column";
            bubbleWrapper.style.alignItems = isMine ? "flex-end" : "flex-start";

            var bubble = document.createElement("div");
            bubble.style.maxWidth = "80%";
            bubble.style.padding = "8px 12px";
            bubble.style.borderRadius = isMine ? "14px 14px 2px 14px" : "14px 14px 14px 2px";
            bubble.style.fontSize = "0.86rem";
            bubble.style.wordBreak = "break-word";

            if (isMine) {
                bubble.style.background = "#ee96a4";
                bubble.style.color = "#fff";
            } else {
                bubble.style.background = "#FFFFFF";
                bubble.style.color = "#1E293B";
                bubble.style.border = "1px solid #E2E8F0";
            }
            bubble.textContent = text;

            var time = document.createElement("span");
            time.style.fontSize = "0.68rem";
            time.style.color = "#94A3B8";
            time.style.marginTop = "2px";
            time.textContent = timeStr || "Just now";

            bubbleWrapper.appendChild(bubble);
            bubbleWrapper.appendChild(time);
            thread.appendChild(bubbleWrapper);
            thread.scrollTop = thread.scrollHeight;
        },

        useIcebreaker: function (text) {
            var input = document.getElementById("globalChatInput");
            if (input) {
                input.value = text;
                window.GlobalStudyChat.send();
            }
        },

        send: function () {
            var input = document.getElementById("globalChatInput");
            if (!input || !activeChatUserId) return;
            var text = input.value.trim();
            if (!text) return;

            input.value = "";

            fetch("/Community/SendMessage", {
                method: "POST",
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded",
                    "RequestVerificationToken": getCsrfToken()
                },
                body: new URLSearchParams({
                    "receiverUserId": activeChatUserId,
                    "content": text,
                    "__RequestVerificationToken": getCsrfToken()
                })
            })
            .then(function (res) { return res.json(); })
            .then(function (data) {
                if (data.success && data.message) {
                    window.GlobalStudyChat.appendBubble(data.message.content, true, data.message.sentAtFormatted);
                    loadNotifications();
                } else {
                    alert(data.message || "Message could not be sent.");
                }
            })
            .catch(function () {
                alert("Could not send message.");
            });
        }
    };

    // Mark all as read button
    if (markAllBtn) {
        markAllBtn.addEventListener("click", function (e) {
            e.preventDefault();
            fetch("/Notifications/MarkAsRead", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": getCsrfToken()
                },
                body: JSON.stringify({})
            }).then(function (res) {
                if (!res.ok) throw new Error("HTTP " + res.status);
                if (badge) badge.style.display = "none";
                if (tabAlertsCount) tabAlertsCount.style.display = "none";
                document.querySelectorAll(".na-notif-item--unread").forEach(function (item) {
                    item.classList.remove("na-notif-item--unread");
                });
            }).catch(function () {});
        });
    }

    // Initial load
    loadNotifications();
    loadChats();

    // Auto poll every 30 seconds (skipped while the tab is in the background)
    // Disabled on the admin workbench (see _Layout admin branch) to avoid background hitches.
    if (!window.naDisableNotificationPolling) {
        setInterval(function () {
            if (document.hidden) return;
            loadNotifications();
            if (activeTab === "messages") {
                loadChats();
            }
            if (activeChatUserId) {
                window.GlobalStudyChat.loadHistory(activeChatUserId);
            }
        }, 30000);
    }
})();
