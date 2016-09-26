#include <stdint.h>

#define xstr(a) str(a)
#define str(a) #a

#define REG32(x)            (*((volatile uint32_t *)(x)))

#define BIT_0               0x00000001
#define BIT_1               0x00000002
#define BIT_2               0x00000004
#define BIT_3               0x00000008
#define BIT_4               0x00000010
#define BIT_5               0x00000020
#define BIT_6               0x00000040
#define BIT_7               0x00000080
#define BIT_8               0x00000100
#define BIT_9               0x00000200
#define BIT_10              0x00000400
#define BIT_11              0x00000800
#define BIT_12              0x00001000
#define BIT_13              0x00002000
#define BIT_14              0x00004000
#define BIT_15              0x00008000
#define BIT_16              0x00010000
#define BIT_17              0x00020000
#define BIT_18              0x00040000
#define BIT_19              0x00080000
#define BIT_20              0x00100000
#define BIT_21              0x00200000
#define BIT_22              0x00400000
#define BIT_23              0x00800000
#define BIT_24              0x01000000
#define BIT_25              0x02000000
#define BIT_26              0x04000000
#define BIT_27              0x08000000
#define BIT_28              0x10000000
#define BIT_29              0x20000000
#define BIT_30              0x40000000
#define BIT_31              0x80000000

#define BK3000_MFC_BASE_ADDR    0x800000
#define BK3000_ICU_BASE_ADDR    0x920000
#define BK3000_WDT_BASE_ADDR    0x970000

#define BK3000_ICU_REG0_SYS_CLOCK_CONFIG        REG32(BK3000_ICU_BASE_ADDR+0x00)
#define BK3000_ICU_REG1_CPU_CLOCK_CONFIG        REG32(BK3000_ICU_BASE_ADDR+0x04)
#define BK3000_ICU_REG6_WDT_CLOCK_CONFIG        REG32(BK3000_ICU_BASE_ADDR+0x18)

#define BK3000_MFC_KEYWORD                      REG32(BK3000_MFC_BASE_ADDR+0x0)
#define BK3000_MFC_CTL                          REG32(BK3000_MFC_BASE_ADDR+0x4)
#define BK3000_MFC_ADDR                         REG32(BK3000_MFC_BASE_ADDR+0x8)
#define BK3000_MFC_DATA                         REG32(BK3000_MFC_BASE_ADDR+0xC)
#define BK3000_MFC_WE_P1                        REG32(BK3000_MFC_BASE_ADDR+0x10)
#define BK3000_MFC_WE_P2                        REG32(BK3000_MFC_BASE_ADDR+0x14)
#define BK3000_MFC_WE_P3                        REG32(BK3000_MFC_BASE_ADDR+0x18)

#define MAIN_SPACE 0x00
#define NVR_SPACE 0x1
#define RDN_SPACE 0x2

#define REG_APB7_WDT_CFG                        REG32(BK3000_WDT_BASE_ADDR+0x00)

#define FLASH_START                             REG32(0x403E00)
#define FLASH_LEN                               REG32(0x403E04)

void __swi(0xFE) disable_isr (void);
void __swi(0xFF) enable_isr (void);

void driver_mfc_write_dword(uint8_t space_control, uint32_t flash_address, uint32_t flash_data)
{
    BK3000_MFC_WE_P1=0XA5;
    BK3000_MFC_WE_P2=0XC3;
    BK3000_MFC_DATA = flash_data;
    BK3000_MFC_ADDR = (flash_address/4);
    BK3000_MFC_KEYWORD = 0x58a9;
    BK3000_MFC_KEYWORD = 0xa958;
    BK3000_MFC_CTL = (1<<2);
    BK3000_MFC_CTL |= (space_control<<5);
    BK3000_MFC_CTL |= (1<<0);
    while(BK3000_MFC_CTL&BIT_0);
    //BK3000_MFC_WE_P2=0X00;
    //BK3000_MFC_WE_P1=0X00;
}

uint32_t driver_mfc_read_dword(uint8_t space_control, uint32_t flash_address)
{
    BK3000_MFC_ADDR = (flash_address/4);
    BK3000_MFC_KEYWORD = 0x58a9;
    BK3000_MFC_KEYWORD = 0xa958;
    BK3000_MFC_CTL = (0<<2);
    BK3000_MFC_CTL |= (space_control<<5);
    BK3000_MFC_CTL |= (1<<0);
    while(BK3000_MFC_CTL&BIT_0);
    return BK3000_MFC_DATA;
}

void driver_mfc_erase_sector(uint8_t space_control, uint32_t flash_address)
{
    BK3000_MFC_WE_P1=0XA5;
    BK3000_MFC_WE_P2=0XC3;
  
    BK3000_MFC_ADDR = (flash_address/4);
    BK3000_MFC_KEYWORD = 0x58a9;
    BK3000_MFC_KEYWORD = 0xa958;
    BK3000_MFC_CTL = (2<<2);
    BK3000_MFC_CTL |= (space_control<<5);
    BK3000_MFC_CTL |= (1<<0);
    while(BK3000_MFC_CTL&BIT_0);
    //BK3000_MFC_WE_P2=0X00;
    //BK3000_MFC_WE_P1=0X00;
}

__asm void init(void)
{
    MOV r0, #0x400000;
    ADD r0, #0x3D00;
    MOV sp, r0;
    BX  lr;
}

void start(void) __attribute__((section(".ARM.__at_" xstr(LOADER_ADDR))));

void start(void)
{
    init();
    //disable_isr();
    // enable 16 MHz clock
    BK3000_ICU_REG0_SYS_CLOCK_CONFIG=0x01;
    BK3000_ICU_REG1_CPU_CLOCK_CONFIG=0x00;
    // disable watchdog
    REG_APB7_WDT_CFG = 0x5A0000;
    REG_APB7_WDT_CFG = 0xA50000;
    BK3000_ICU_REG6_WDT_CLOCK_CONFIG=1;

    uint32_t flash_addr = FLASH_START;
    uint32_t len = FLASH_LEN;
    if (flash_addr + len > 0x40000)
    {
        __breakpoint(0x0001);
        for (;;) { }
    }
    for (uint32_t i = 0; i < len; i += 4)
    {
        if (flash_addr >= LOADER_ADDR)
        {   // prevent override own code
            break;
        }
        if ((flash_addr & 0xFF) == 0x00)
        {
            driver_mfc_erase_sector(MAIN_SPACE, flash_addr);
        }
        driver_mfc_write_dword(MAIN_SPACE, flash_addr, REG32(i + 0x400000));
        flash_addr += 4;
    }
    BK3000_MFC_WE_P2=0X00;
    BK3000_MFC_WE_P1=0X00;
    __breakpoint(0x0000);
    for (;;) { }
}
