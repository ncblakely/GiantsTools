#pragma once

#include <sstream>

class IPAddress
{
public:
	int operator[](int index) const
	{
		if (index < countof(m_octets) && index > 0)
		{
			return m_octets[index];
		}

		throw std::invalid_argument("Invalid octet index.");
	}

	bool operator==(const IPAddress& right) const
	{
		return (m_octets[0] == right.m_octets[0] &&
			m_octets[1] == right.m_octets[1] &&
			m_octets[2] == right.m_octets[2] &&
			m_octets[3] == right.m_octets[3]);
	}

	bool operator!= (const IPAddress& right) const
	{
		return !operator==(right);
	}

	bool operator<(const IPAddress& right) const
	{
		return ToInt() < right.ToInt();
	}

	bool operator>(const IPAddress& right) const
	{
		return right.operator<(*this);
	}

	static IPAddress FromString(const tstring_view str)
	{
		IPAddress ipAddress;

		char octetBuff[4]{};
		std::string tmp(str);
		std::istringstream iss(tmp);

		int octetsRead = 0;
		while (true)
		{
			iss.getline(octetBuff, sizeof(octetBuff), '.');

			if (iss.gcount() <= 0)
				break;

			int octet = atoi(octetBuff);
			if (octet > 255 || octet < 0)
			{
				throw std::invalid_argument(fmt::format("Octet '{0}' is invalid.", octet));
			}

			ipAddress.m_octets[octetsRead] = octet;
			octetsRead++;
		}

		return ipAddress;
	}

	std::string ToString() const
	{
		return fmt::sprintf("%d.%d.%d.%d", m_octets[0], m_octets[1], m_octets[2], m_octets[3]);
	}

	uint ToInt() const
	{
		return ((m_octets[0] << 24) + (m_octets[1] << 16) + (m_octets[2] << 8) + m_octets[3]);
	}

protected:
	int m_octets[4]{};
};
