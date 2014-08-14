// This template is a compilation and simplified version of
// a collection of online examples. It is to be used  as  a
// building block in network programming.

#include <iostream>
#include <cstring>      // Needed for memset
#include <sys/socket.h> // Needed for the socket functions
#include <netdb.h>      // Needed for the socket functions

int main()
{
    int status;
    struct sockaddr_in serv_addr;

    memset(&serv_addr, 0, sizeof(serv_addr)));

	// set socket properties
    serv_addr.sin_family = AF_INET;
    serv_addr.sin_addr.s_addr = inet_addr("10.0.0.2");
    serv_addr.sin_port = htons(8421);
	

    int socketfd ; // The socket descripter
    socketfd = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
    if (socketfd == -1)  std::cout << "socket error " ;

    status = connect(socketfd, &serv_addr, sizeof(serv_addr));
    if (status == -1)  std::cout << "connect error" ;

    while(1)
    {
	    char *msg = "GET / HTTP/1.1\nhost: www.google.com\n\n";
	    int len;
	    ssize_t bytes_sent;
	    len = strlen(msg);
	    bytes_sent = send(socketfd, msg, len, 0);

	    ssize_t bytes_recieved;
	    char incomming_data_buffer[1000];
	    bytes_recieved = recv(socketfd, incomming_data_buffer,1000, 0);
	    // If no data arrives, the program will just wait here until some data arrives.
	    if (bytes_recieved == 0) 
	    {
	    	std::cout << "host shut down." << std::endl ;
	    	break;
	    }
	    if (bytes_recieved == -1)std::cout << "recieve error!" << std::endl ;
	    std::cout << bytes_recieved << " bytes recieved :" << std::endl ;
	    incomming_data_buffer[bytes_recieved] = '\0' ;

	    std::cout << incomming_data_buffer << std::endl;
	}

    close(socketfd);
