package com.drunkengames.hangover.domain.rank.service;

import com.drunkengames.hangover.domain.rank.dto.RankDto;
import com.drunkengames.hangover.domain.rank.dto.RankRequestDto;
import com.drunkengames.hangover.domain.rank.dto.RankResponseDto;

import java.util.List;

/**
 * <pre>
 *     랭킹 관리 서비스 인터페이스
 * </pre>
 *
 * @author 박봉균
 * @since JDK17 Eclipse Temurin
 */

public interface RankService {
    void insertRank(RankRequestDto rankRequestDto) throws Exception;
    List<RankDto> listRank() throws Exception;
}
