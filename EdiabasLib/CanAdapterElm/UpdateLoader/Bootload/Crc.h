#ifndef CRC_H
#define CRC_H

/*!
 * Calculates 16-bit CCITT CRC values using fast lookup table algorithm.
 */
class Crc
{
public:
    Crc(unsigned short init = 0);
    void Add(unsigned char byte);

    unsigned char MSB(void);
    unsigned char LSB(void);
    unsigned short Value(void);

protected:
    unsigned short crc;
    static const unsigned short table[256];
};

#endif // CRC_H
