#define _USE_MATH_DEFINES
#include <cmath>
#include "Biquad.h"

namespace AudioLib
{
	Biquad::Biquad() 
	{
		ClearBuffers();
	}

	Biquad::Biquad(FilterType filterType, double samplerate)
	{
		Type = filterType;
		SetSamplerate(samplerate);

		SetGainDb(0.0);
		Frequency = samplerate / 4;
		SetQ(0.5);
		ClearBuffers();
	}

	Biquad::~Biquad() 
	{

	}


	double Biquad::GetSamplerate() 
	{
		return samplerate;
	}

	void Biquad::SetSamplerate(double value)
	{
		samplerate = value; 
		Update();
	}

	double Biquad::GetGainDb() 
	{
		return std::log10(gain) * 20;
	}

	void Biquad::SetGainDb(double value) 
	{
		SetGain(std::pow(10, value / 20));
	}

	double Biquad::GetGain() 
	{
		return gain;
	}

	void Biquad::SetGain(double value) 
	{
		if (value == 0)
			value = 0.001; // -60dB
		
		gain = value;
	}

	double Biquad::GetQ()
	{
		return _q;
	}

	void Biquad::SetQ(double value) 
	{
		if (value == 0)
			value = 1e-12;
		_q = value;
	}

	vector<double> Biquad::GetA() 
	{
		return vector<double>({ 1, a1, a2 });
	}

	vector<double> Biquad::GetB()
	{
		return vector<double>({ b0, b1, b2 });
	}


	void Biquad::Update()
	{
		double omega = 2 * M_PI * Frequency / samplerate;
		double sinOmega = std::sin(omega);
		double cosOmega = std::cos(omega);

		double sqrtGain = 0.0;
		double alpha = 0.0;

		if (Type == FilterType::LowShelf || Type == FilterType::HighShelf)
		{
			alpha = sinOmega / 2 * std::sqrt((gain + 1 / gain) * (1 / Slope - 1) + 2);
			sqrtGain = std::sqrt(gain);
		}
		else
		{
			alpha = sinOmega / (2 * _q);
		}

		switch (Type)
		{
		case FilterType::LowPass:
			b0 = (1 - cosOmega) / 2;
			b1 = 1 - cosOmega;
			b2 = (1 - cosOmega) / 2;
			a0 = 1 + alpha;
			a1 = -2 * cosOmega;
			a2 = 1 - alpha;
			break;
		case FilterType::HighPass:
			b0 = (1 + cosOmega) / 2;
			b1 = -(1 + cosOmega);
			b2 = (1 + cosOmega) / 2;
			a0 = 1 + alpha;
			a1 = -2 * cosOmega;
			a2 = 1 - alpha;
			break;
		case FilterType::BandPass:
			b0 = alpha;
			b1 = 0;
			b2 = -alpha;
			a0 = 1 + alpha;
			a1 = -2 * cosOmega;
			a2 = 1 - alpha;
			break;
		case FilterType::Notch:
			b0 = 1;
			b1 = -2 * cosOmega;
			b2 = 1;
			a0 = 1 + alpha;
			a1 = -2 * cosOmega;
			a2 = 1 - alpha;
			break;
		case FilterType::Peak:
			b0 = 1 + (alpha * gain);
			b1 = -2 * cosOmega;
			b2 = 1 - (alpha * gain);
			a0 = 1 + (alpha / gain);
			a1 = -2 * cosOmega;
			a2 = 1 - (alpha / gain);
			break;
		case FilterType::LowShelf:
			b0 = gain * ((gain + 1) - (gain - 1) * cosOmega + 2 * sqrtGain * alpha);
			b1 = 2 * gain * ((gain - 1) - (gain + 1) * cosOmega);
			b2 = gain * ((gain + 1) - (gain - 1) * cosOmega - 2 * sqrtGain * alpha);
			a0 = (gain + 1) + (gain - 1) * cosOmega + 2 * sqrtGain * alpha;
			a1 = -2 * ((gain - 1) + (gain + 1) * cosOmega);
			a2 = (gain + 1) + (gain - 1) * cosOmega - 2 * sqrtGain * alpha;
			break;
		case FilterType::HighShelf:
			b0 = gain * ((gain + 1) + (gain - 1) * cosOmega + 2 * sqrtGain * alpha);
			b1 = -2 * gain * ((gain - 1) + (gain + 1) * cosOmega);
			b2 = gain * ((gain + 1) + (gain - 1) * cosOmega - 2 * sqrtGain * alpha);
			a0 = (gain + 1) - (gain - 1) * cosOmega + 2 * sqrtGain * alpha;
			a1 = 2 * ((gain - 1) - (gain + 1) * cosOmega);
			a2 = (gain + 1) - (gain - 1) * cosOmega - 2 * sqrtGain * alpha;
			break;
		}

		double g = 1 / a0;

		b0 = b0 * g;
		b1 = b1 * g;
		b2 = b2 * g;
		a1 = a1 * g;
		a2 = a2 * g;
	}

	double Biquad::GetResponse(double freq)
	{
		double phi = std::pow((std::sin(2 * M_PI * freq / (2.0 * samplerate))), 2);
		return (std::pow(b0 + b1 + b2, 2.0) - 4.0 * (b0 * b1 + 4.0 * b0 * b2 + b1 * b2) * phi + 16.0 * b0 * b2 * phi * phi) / (std::pow(1.0 + a1 + a2, 2.0) - 4.0 * (a1 + 4.0 * a2 + a1 * a2) * phi + 16.0 * a2 * phi * phi);
	}

	void Biquad::ClearBuffers() 
	{
		y = 0;
		x2 = 0;
		y2 = 0;
		x1 = 0;
		y1 = 0;
	}

}