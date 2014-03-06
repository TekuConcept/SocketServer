//============================================================================
// Name        : SocketServer.cpp
// Author      : TekuConcept
// Version     : 1.0
// Copyright   : Free to use. Give credit where credit is due!
// Description : Socket server in C++, Ansi-style
//============================================================================

#include <iostream>     // time, ctime
#include <cstdlib>      // exit
#include <cstdio>       // snprintf
#include <unistd.h>     // gethostname, write, close, sleep
#include <netdb.h>      // socket, htonl, htons
#include <sstream>
#define MAXHOSTNAME 256

void* memset(void* b, int c, size_t len);
void* memcpy(void* dst, const void* src, size_t count);
bool getBit(char c, int idx);
void setBit(char &c, int idx);
void unsetBit(char &c, int idx);
void toggleBit(char &c, int idx);
void initialize();
void manageData(char* data, int size);

float xdo, ydo;
const int PERIOD = 2000000;

int main()
{
	initialize();

	xdo = 0.0F;
	ydo = 0.0F;

    int listenfd = 0, connfd = 0;
    struct sockaddr_in serv_addr;
    const int BUFF = 32;
    char ioBuff[BUFF];

    // create socket and flush memory blocks
    listenfd = socket(AF_INET, SOCK_STREAM, 0);
    memset(&serv_addr, '0', sizeof(serv_addr));
    memset(ioBuff, (char)0x00, sizeof(ioBuff));

    // set socket properties
    serv_addr.sin_family = AF_INET;
    serv_addr.sin_addr.s_addr = htonl(INADDR_ANY); // 0.0.0.0
    serv_addr.sin_port = htons(8421);

    // Initialize and start the socket
    bind(listenfd, (struct sockaddr*)&serv_addr, sizeof(serv_addr));
    listen(listenfd, 10);

    while(1)
    {
        // receive incoming connection
        connfd = accept(listenfd, (struct sockaddr*)NULL, NULL);

        do
        {
            // read data from client
            read(connfd, ioBuff, BUFF);

            manageData(ioBuff, BUFF-1);

            // write data to client
            write(connfd, ioBuff, BUFF);

            // continue to read and write until '-1' is received
        }while(!getBit(ioBuff[0], 7));

        // close connection and repeat
        close(connfd);
        sleep(10);
    }
}

void* memset(void* b, int c, size_t len) {
    char* p = (char*)b;
    for (size_t i = 0; i != len; ++i) {
        p[i] = c;
    }
    return b;
}
void* memcpy(void* dst, const void* src, size_t count) {
	void * ret = dst;
	while(count--) {
		*(char *)dst = *(char *)src;
		dst = (char *)dst + 1;
		src = (char *)src + 1;
	}
	return(ret);
}
bool getBit(char c, int idx)
{
	return (c >> idx) & 1;
}
void setBit(char &c, int idx)
{
	c |= 1 << idx;
}
void unsetBit(char &c, int idx)
{
	c &= ~(1 << idx);
}
void toggleBit(char &c, int idx)
{
	c ^= 1 << idx;
}


void initialize()
{
	std::ostringstream sos("");

	// create gpio pins
	sos << "echo 30 > /sys/class/gpio/export;";
	sos << "echo 31 > /sys/class/gpio/export;";
	sos << "echo 48 > /sys/class/gpio/export;";
	sos << "echo 60 > /sys/class/gpio/export;";

	// set gpio directions
	sos << "echo low > /sys/class/gpio/gpio30/direction;";
	sos << "echo low > /sys/class/gpio/gpio31/direction;";
	sos << "echo low > /sys/class/gpio/gpio48/direction;";
	sos << "echo low > /sys/class/gpio/gpio60/direction;";

	/*
	// set up environment for first time (pwm, spi, i2c)
	sos << "echo am33xx_pwm > /sys/devices/bone_capemgr.9/slots;";

	// create pwm pins
	sos << "echo bone_pwm_P8_13 > /sys/devices/bone_capemgr.9/slots;";
	sos << "echo bone_pwm_P9_14 > /sys/devices/bone_capemgr.9/slots;";
	*/
	system(sos.str().c_str());
	//sos.flush();

	/*
	// set pwm period and default duty cycle (2ms)
	sos << "echo " << PERIOD << " > /sys/devices/ocp.3/pwm_test_P8_13.15/period;";
	sos << "echo " << PERIOD << " > /sys/devices/ocp.3/pwm_test_P8_13.15/duty;";

	sos << "echo " << PERIOD << " > /sys/devices/ocp.3/pwm_test_P9_14.16/period;";
	sos << "echo " << PERIOD << " > /sys/devices/ocp.3/pwm_test_P9_14.16/duty;";
	system(sos.str().c_str());
	*/
}
void manageData(char* data, int size)
{
	std::ostringstream ss("");

	// gpio data
	ss << "echo " << getBit(data[0], 0) << " > /sys/class/gpio/gpio30/value && ";
	ss << "echo " << getBit(data[0], 1) << " > /sys/class/gpio/gpio31/value && ";
	ss << "echo " << getBit(data[0], 2) << " > /sys/class/gpio/gpio48/value && ";
	ss << "echo " << getBit(data[0], 3) << " > /sys/class/gpio/gpio60/value;";

	/*
	// pwm data
	bool cf = getBit(data[0], 4),
		 zf = getBit(data[0], 5);
	float xd, yd;
	if(zf && !cf) // set respective duty cycles
	{
		char tx[] = { data[1], data[2], data[3], data[4] };
		char ty[] = { data[5], data[6], data[7], data[8] };

		memcpy(&xd, &tx, sizeof(float));
		memcpy(&yd, &ty, sizeof(float));
		xd *= (xd < 0 ? -1 : 1); // get absolute value
		yd *= (yd < 0 ? -1 : 1);
		if(xd > 1) xd = 1; // avoid overflow
		if(yd > 1) yd = 1;

		// 100% = 0% duty cycle; invert input percentage so that 100% is now 100% duty cycle
		//resc << "x: " << xd << "\ty: " << yd;
		ss << "echo " << PERIOD - (int)(PERIOD * xd) << " > /sys/devices/ocp.3/pwm_test_P8_13.15/duty;";
		ss << "echo " << PERIOD - (int)(PERIOD * yd) << " > /sys/devices/ocp.3/pwm_test_P9_14.16/duty;";
	}
	else if(zf && cf) // reset duty cycles (stop / 0)
	{
		xd = 0.0F;
		yd = 0.0F;
		ss << "echo " << PERIOD << " > /sys/devices/ocp.3/pwm_test_P8_13.15/duty;";
		ss << "echo " << PERIOD << " > /sys/devices/ocp.3/pwm_test_P9_14.16/duty;";
	}
	xdo = xd;
	ydo = yd;
	*/

	system(ss.str().c_str());
}

