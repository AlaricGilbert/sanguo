# Network framework

The network connection workflow is as following:

![NetworkFramework](..\workflow\image\NetworkFramework.png)

<p style="text-align: center;"><b>Picture: How does a user connect.</b></p>

Firstly, the user should connect to a `HubServer`, which provides interface of finding `LoginServer` and `GameHoldingServer`'s location (After a `LoginServer` or `GameHoldingServer` has been launched, it should also report its IP and port to the `HubServer`; the `HubServer` should bear the mission of balance the pressure of user connecting.). And with the provided information, the client can login to the lobbies and start gaming.

